"""Lo que comparten las pruebas de humo de la API (v2 en adelante).

Cada bloque tiene su propio guion —humo_academico.py, humo_actividades.py,
humo_puentes.py— para que los tres estudiantes no editen el mismo archivo.
Aquí vive solo lo común: cómo llamar a la API y cómo llevar la cuenta de lo
que falló.

Las pruebas de humo hablan con la API REAL, levantada con
`docker compose up -d --build`. Son el complemento de la prueba de capas: esa
prueba los servicios sin base de datos; estas, el sistema completo.
"""
import json
import os
import random
import sys
import urllib.error
import urllib.request

API = "http://localhost:8072"

_fallos = []
_comprobaciones = 0

# v3: desde que la API exige JWT, los guiones de la v2 siguen funcionando sin
# cambios porque api() inicia sesión SOLA como administrador la primera vez
# que recibe un 401 (con ADMIN_USUARIO y ADMIN_CLAVE del .env: los guiones no
# tienen ninguna credencial escrita). Para probar a propósito sin token o con
# otro rol, se pasa token= explícito (ver humo_seguridad.py).
_token_admin = None
_SIN_TOKEN = object()


def leer_env(nombre):
    """Lee una variable del .env de la raíz del proyecto."""
    ruta = os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", ".env")
    with open(ruta, encoding="utf-8") as f:
        for linea in f:
            if linea.startswith(nombre + "="):
                return linea.strip().split("=", 1)[1]
    return None


def iniciar_sesion(usuario, clave):
    """POST /api/login. Devuelve el token, o None si no se pudo."""
    codigo, cuerpo = _llamar("POST", "/api/login", {"usuario": usuario, "clave": clave}, None)
    return cuerpo.get("token") if codigo == 200 and cuerpo else None


def api(metodo, ruta, cuerpo=None, token=_SIN_TOKEN):
    """Llama a la API y devuelve (código, cuerpo como dict o None).

    Sin `token`, usa el del administrador (y lo consigue si hace falta). Con
    token=None la petición va SIN token; con un texto, va con ese token."""
    global _token_admin
    if token is not _SIN_TOKEN:
        return _llamar(metodo, ruta, cuerpo, token)
    codigo, respuesta = _llamar(metodo, ruta, cuerpo, _token_admin)
    if codigo == 401 and _token_admin is None and leer_env("ADMIN_USUARIO"):
        _token_admin = iniciar_sesion(leer_env("ADMIN_USUARIO"), leer_env("ADMIN_CLAVE"))
        if _token_admin:
            codigo, respuesta = _llamar(metodo, ruta, cuerpo, _token_admin)
    return codigo, respuesta


def _llamar(metodo, ruta, cuerpo, token):
    datos = json.dumps(cuerpo).encode("utf-8") if cuerpo is not None else None
    peticion = urllib.request.Request(API + ruta, data=datos, method=metodo)
    peticion.add_header("Content-Type", "application/json")
    if token:
        peticion.add_header("Authorization", "Bearer " + token)
    try:
        with urllib.request.urlopen(peticion, timeout=30) as r:
            texto = r.read().decode("utf-8")
            return r.status, (json.loads(texto) if texto else None)
    except urllib.error.HTTPError as e:
        texto = e.read().decode("utf-8", "replace")
        try:
            return e.code, (json.loads(texto) if texto else None)
        except ValueError:
            return e.code, {"texto": texto}
    except Exception as e:  # la API no responde
        return 0, {"error": str(e)}


def revisar(condicion, descripcion, detalle=None):
    """La línea que puede fallar: comprueba, narra y deja constancia."""
    global _comprobaciones
    _comprobaciones += 1
    if condicion:
        print(f"  [OK] {descripcion}")
    else:
        print(f"  [ERROR] {descripcion}" + (f"  ->  {detalle}" if detalle is not None else ""))
        _fallos.append(descripcion)


def esperar(codigo_esperado, metodo, ruta, cuerpo=None, descripcion=None, token=_SIN_TOKEN):
    """Llama y comprueba el código de estado. Devuelve el cuerpo."""
    codigo, respuesta = api(metodo, ruta, cuerpo, token)
    revisar(codigo == codigo_esperado,
            descripcion or f"{metodo} {ruta} -> {codigo_esperado}",
            f"llegó {codigo}: {respuesta}")
    return respuesta


def llave_aleatoria(prefijo=7):
    """Una llave distinta en cada corrida: si una se interrumpe a la mitad deja
    registros puestos, y la siguiente fallaría por llave duplicada."""
    return int(f"{prefijo}{random.randint(10000, 99999)}")


def terminar(nombre):
    print()
    if _fallos:
        print(f"=== {nombre}: {len(_fallos)} de {_comprobaciones} comprobaciones FALLARON ===")
        for f in _fallos:
            print(f"   - {f}")
        sys.exit(1)
    print(f"=== {nombre}: las {_comprobaciones} comprobaciones pasaron ===")
    sys.exit(0)
