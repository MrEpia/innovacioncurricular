"""Prueba de humo de la v3 — autenticación JWT y autorización por roles —
contra la API real.

Responsable: Sebastián Calvo.

Comprueba los criterios de la spec de la v3: login correcto e incorrecto,
token ausente, adulterado, firmado con otra llave y VENCIDO; los permisos de
administrador, editor y consulta; la protección de /api/usuario, /api/rol y
/api/rol_usuario; usuarios retirados y sin roles; y que la contraseña se
guarda hasheada.

El administrador y la llave de firma salen del .env: el guion no tiene ninguna
credencial escrita.

Uso:  python pruebas_humo/humo_seguridad.py     (parado en la raíz del proyecto)
"""
import base64
import hashlib
import hmac
import json
import subprocess
import time

from comun import api, esperar, iniciar_sesion, leer_env, llave_aleatoria, revisar, terminar


def b64(datos):
    return base64.urlsafe_b64encode(datos).rstrip(b"=").decode()


def token_a_mano(secreto, minutos_desde_ahora, roles=("administrador",)):
    """Fabrica un JWT HS256 con la librería estándar de Python. Sirve para
    probar un token VENCIDO sin esperar una hora, y uno firmado con otra llave."""
    ahora = int(time.time())
    cabecera = {"alg": "HS256", "typ": "JWT"}
    contenido = {"sub": "1", "unique_name": "falso", "role": list(roles),
                 "iss": "innovacion-api", "aud": "innovacion-front",
                 "nbf": ahora - 7200, "exp": ahora + minutos_desde_ahora * 60}
    base = b64(json.dumps(cabecera).encode()) + "." + b64(json.dumps(contenido).encode())
    firma = hmac.new(secreto.encode(), base.encode(), hashlib.sha256).digest()
    return base + "." + b64(firma)


def hash_en_base(username):
    clave_sa = leer_env("MSSQL_SA_PASSWORD")
    salida = subprocess.run(
        ["docker", "exec", "innovacion-sqlserver", "/opt/mssql-tools18/bin/sqlcmd", "-S", "localhost",
         "-U", "sa", "-P", clave_sa, "-C", "-d", "innovacion_local", "-h", "-1", "-W", "-Q",
         f"SET NOCOUNT ON; SELECT password FROM usuario WHERE username = '{username}'"],
        capture_output=True, text=True)
    return salida.stdout.strip()


print("=== Humo v3 · seguridad ===")

print("\n-- sin token")
_, diag = api("GET", "/", token=None)
revisar(diag and diag.get("version") == "v3", "GET / responde v3 sin token (diagnóstico)", diag)
cuerpo = esperar(401, "GET", "/api/universidad", token=None, descripcion="un recurso sin token → 401")
revisar(cuerpo and "Falta el token" in cuerpo.get("detalle", ""), "el 401 dice que falta el token", cuerpo)

print("\n-- login")
esperar(422, "POST", "/api/login", {"usuario": "admin"}, "login sin clave → 422", token=None)
cuerpo = esperar(401, "POST", "/api/login", {"usuario": leer_env("ADMIN_USUARIO"), "clave": "equivocada"},
                 "clave equivocada → 401", token=None)
mensaje_clave = (cuerpo or {}).get("detalle")
cuerpo = esperar(401, "POST", "/api/login", {"usuario": "no_existe_nadie", "clave": "x"},
                 "usuario inexistente → 401", token=None)
revisar(cuerpo and cuerpo.get("detalle") == mensaje_clave,
        "usuario inexistente y clave equivocada dan EL MISMO mensaje (no se enumeran usuarios)", cuerpo)
ingreso = esperar(200, "POST", "/api/login",
                  {"usuario": leer_env("ADMIN_USUARIO"), "clave": leer_env("ADMIN_CLAVE")},
                  "login del administrador del .env → 200", token=None)
ADMIN = ingreso["token"]
ADMIN_ID = ingreso["usuario"]["id"]
revisar("administrador" in ingreso["usuario"]["roles"] and ingreso["tipo"] == "Bearer",
        "el login devuelve el token Bearer y los roles", ingreso)
esperar(200, "GET", "/api/universidad", token=ADMIN, descripcion="con el token, el recurso responde 200")

print("\n-- tokens que NO sirven")
cuerpo = esperar(401, "GET", "/api/universidad", token=ADMIN[:-3] + "abc", descripcion="token adulterado → 401")
revisar(cuerpo and "no es válido" in cuerpo.get("detalle", ""), "el 401 dice que el token no es válido", cuerpo)
esperar(401, "GET", "/api/universidad", token=token_a_mano("otra-llave-de-al-menos-32-caracteres!!", 30),
        descripcion="token firmado con OTRA llave → 401")
cuerpo = esperar(401, "GET", "/api/universidad", token=token_a_mano(leer_env("JWT_SECRETO"), -10),
                 descripcion="token con la llave correcta pero VENCIDO → 401")
revisar(cuerpo and "expiró" in cuerpo.get("detalle", ""), "el 401 dice que el token expiró", cuerpo)
esperar(200, "GET", "/api/universidad", token=token_a_mano(leer_env("JWT_SECRETO"), 10),
        descripcion="control: el mismo token SIN vencer → 200")

print("\n-- usuarios y roles (solo administrador)")
roles = {r["nombre"]: r["id"] for r in esperar(200, "GET", "/api/rol", token=ADMIN)["datos"]}
revisar({"administrador", "editor", "consulta"} <= set(roles), "existen los tres roles del sistema", roles)
sufijo = llave_aleatoria(1)
USUARIOS = {}
for nombre in ("consulta", "editor", "sinrol"):
    username = f"humo_{nombre}_{sufijo}"
    r = esperar(201, "POST", "/api/usuario",
                {"username": username, "clave": f"Clave_{nombre}_123", "email": f"{username}@prueba.co",
                 "nombreCompleto": f"Usuario {nombre}"}, f"crear usuario {nombre} → 201", token=ADMIN)
    USUARIOS[nombre] = (r["llave"]["id"], username, f"Clave_{nombre}_123")
esperar(409, "POST", "/api/usuario", {"username": USUARIOS["editor"][1], "clave": "Otra_Clave_1",
                                      "email": "otro@prueba.co"}, "username repetido → 409", token=ADMIN)
esperar(422, "POST", "/api/usuario", {"username": "x", "clave": "corta", "email": "no-es-correo"},
        "clave corta y correo inválido → 422", token=ADMIN)
revisar(hash_en_base(USUARIOS["editor"][1]).startswith("pbkdf2-sha256$"),
        "en la base la contraseña está HASHEADA (pbkdf2-sha256$…), no en texto plano")
listado = esperar(200, "GET", "/api/usuario", token=ADMIN)
revisar(all("password" not in u and "clave" not in u for u in listado["datos"]),
        "el listado de usuarios no expone contraseñas ni hashes")

esperar(201, "POST", "/api/rol_usuario", {"usuarioId": USUARIOS["consulta"][0], "rolId": roles["consulta"]}, token=ADMIN)
esperar(201, "POST", "/api/rol_usuario", {"usuarioId": USUARIOS["editor"][0], "rolId": roles["editor"]}, token=ADMIN)
esperar(409, "POST", "/api/rol_usuario", {"usuarioId": USUARIOS["editor"][0], "rolId": roles["editor"]},
        "asignar dos veces el mismo rol → 409", token=ADMIN)
esperar(400, "POST", "/api/rol_usuario", {"usuarioId": 999999, "rolId": roles["editor"]},
        "asignar a un usuario inexistente → 400", token=ADMIN)

CONSULTA = iniciar_sesion(USUARIOS["consulta"][1], USUARIOS["consulta"][2])
EDITOR = iniciar_sesion(USUARIOS["editor"][1], USUARIOS["editor"][2])
revisar(CONSULTA and EDITOR, "consulta y editor pueden iniciar sesión")
esperar(403, "POST", "/api/login", {"usuario": USUARIOS["sinrol"][1], "clave": USUARIOS["sinrol"][2]},
        "un usuario sin ningún rol → 403", token=None)

print("\n-- permisos por rol (aplicados en el BACKEND)")
EN = llave_aleatoria(1)
ENFOQUE = {"id": EN, "nombre": "Enfoque de humo", "descripcion": "Prueba de permisos"}
esperar(200, "GET", "/api/universidad", token=CONSULTA, descripcion="consulta puede LEER → 200")
esperar(403, "POST", "/api/enfoque", ENFOQUE, "consulta NO puede crear → 403", token=CONSULTA)
esperar(200, "POST", "/api/enfoque", ENFOQUE, "editor SÍ puede crear → 200", token=EDITOR)
esperar(200, "PATCH", f"/api/enfoque/{EN}", {"descripcion": "Editado por el editor"},
        "editor SÍ puede modificar → 200", token=EDITOR)
esperar(403, "PATCH", f"/api/enfoque/{EN}", {"descripcion": "x"}, "consulta NO puede modificar → 403", token=CONSULTA)
esperar(403, "DELETE", f"/api/enfoque/{EN}", descripcion="editor NO puede retirar → 403", token=EDITOR)
esperar(403, "DELETE", f"/api/enfoque/{EN}", descripcion="consulta NO puede retirar → 403", token=CONSULTA)
esperar(200, "DELETE", f"/api/enfoque/{EN}", descripcion="administrador SÍ puede retirar → 200", token=ADMIN)
for recurso in ("usuario", "rol", "rol_usuario"):
    esperar(403, "GET", f"/api/{recurso}", descripcion=f"editor no ve /api/{recurso} → 403", token=EDITOR)
    esperar(403, "GET", f"/api/{recurso}", descripcion=f"consulta no ve /api/{recurso} → 403", token=CONSULTA)

print("\n-- reglas de seguridad")
esperar(409, "DELETE", f"/api/usuario/{ADMIN_ID}", descripcion="el único administrador no se puede retirar → 409", token=ADMIN)
esperar(409, "DELETE", f"/api/rol/{roles['administrador']}", descripcion="el rol administrador no se retira → 409", token=ADMIN)
esperar(200, "DELETE", f"/api/rol_usuario/{USUARIOS['consulta'][0]}/{roles['consulta']}",
        "quitar un rol → 200", token=ADMIN)
esperar(403, "POST", "/api/login", {"usuario": USUARIOS["consulta"][1], "clave": USUARIOS["consulta"][2]},
        "sin su único rol, ya no puede ingresar → 403", token=None)
esperar(200, "DELETE", f"/api/usuario/{USUARIOS['editor'][0]}", descripcion="retirar al editor → 200", token=ADMIN)
esperar(401, "POST", "/api/login", {"usuario": USUARIOS["editor"][1], "clave": USUARIOS["editor"][2]},
        "un usuario retirado no puede ingresar → 401", token=None)

print("\n-- limpieza")
for nombre in ("consulta", "sinrol"):
    esperar(200, "DELETE", f"/api/usuario/{USUARIOS[nombre][0]}", token=ADMIN)

terminar("Humo v3 · seguridad")
