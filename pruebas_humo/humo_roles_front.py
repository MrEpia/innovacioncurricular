"""Prueba de humo del FRONT por ROL y del CIERRE DE SESIÓN (entrega 3).

humo_front.py prueba el ingreso con el administrador. Este guion completa lo
que el PDF pide del front y que solo se ve entrando con OTROS roles:

  · menú dinámico: «Administración» solo aparece para el administrador;
  · control de acceso: «consulta» no ve «Agregar»; editor y administrador sí;
  · el panel /admin/usuarios: el administrador lo ve, los demás reciben
    «Su rol no permite ver esta pantalla»;
  · logout: después de /sesion/salir, la MISMA cookie ya no abre nada,
    porque la sesión (y el JWT que tenía adentro) se borró del servidor.

Crea sus propios usuarios por la API (con el administrador del .env), entra
con cada uno por el formulario de /ingresar y los retira al final.

Uso:  python pruebas_humo/humo_roles_front.py     (parado en la raíz del proyecto)
"""
import html
import http.cookiejar
import random
import re
import string
import urllib.error
import urllib.parse
import urllib.request

from comun import api, esperar, revisar, terminar

FRONT = "http://localhost:8073"


class _SinRedireccion(urllib.request.HTTPRedirectHandler):
    def redirect_request(self, *args, **kwargs):
        return None


class Navegador:
    """Un navegador con su propia cookie de sesión: uno por usuario."""

    def __init__(self):
        self.cookies = http.cookiejar.CookieJar()
        self.abridor = urllib.request.build_opener(urllib.request.HTTPCookieProcessor(self.cookies))

    def ver(self, ruta, datos=None):
        peticion = urllib.request.Request(FRONT + ruta, data=datos, method="POST" if datos else "GET")
        try:
            with self.abridor.open(peticion, timeout=20) as r:
                return r.status, r.geturl(), r.read().decode("utf-8", "replace")
        except urllib.error.HTTPError as e:
            return e.code, FRONT + ruta, e.read().decode("utf-8", "replace")

    def token_formulario(self, pagina):
        t = re.search(r'name="__RequestVerificationToken"[^>]*value="([^"]+)"', pagina)
        return html.unescape(t.group(1)) if t else ""

    def ingresar(self, usuario, clave):
        _, _, pagina = self.ver("/ingresar")
        manejador = re.search(r'name="_handler"[^>]*value="([^"]+)"', pagina)
        datos = urllib.parse.urlencode({
            "__RequestVerificationToken": self.token_formulario(pagina),
            "_handler": manejador.group(1) if manejador else "ingreso",
            "Datos.Usuario": usuario,
            "Datos.Clave": clave,
        }).encode()
        return self.ver("/ingresar", datos)

    def cookie_sesion(self):
        return next((c.value for c in self.cookies if c.name == ".Innovacion.Sesion"), None)


def visible(pagina):
    sin_script = re.sub(r"(?is)<(script|style)[^>]*>.*?</\1>", " ", pagina)
    return re.sub(r"\s+", " ", html.unescape(re.sub(r"<[^>]*>", " ", sin_script)))


def clave_aleatoria():
    return "Hm-" + "".join(random.choices(string.ascii_letters + string.digits, k=14)) + "9!"


print("=== Humo v3 · front por rol y cierre de sesión ===")

_, roles = api("GET", "/api/rol")
ROL = {r["nombre"]: r["id"] for r in (roles or {}).get("datos", [])}
revisar(all(n in ROL for n in ("administrador", "editor", "consulta")), "existen los tres roles del sistema", ROL)

USUARIOS = {}
for rol in ("consulta", "editor", "administrador"):
    nombre = f"humo_{rol[:4]}_{random.randint(10000, 99999)}"
    clave = clave_aleatoria()
    creado = esperar(201, "POST", "/api/usuario",
                     {"username": nombre, "clave": clave, "email": f"{nombre}@humo.test",
                      "nombreCompleto": f"Humo {rol}"}, f"crear un usuario «{rol}» → 201")
    uid = (creado or {}).get("llave", {}).get("id")
    esperar(201, "POST", "/api/rol_usuario", {"usuarioId": uid, "rolId": ROL.get(rol)},
            f"asignarle el rol «{rol}» → 201")
    USUARIOS[rol] = (uid, nombre, clave)

NAVEGADORES = {}
for rol, (uid, nombre, clave) in USUARIOS.items():
    print(f"\n-- entrando como «{rol}»")
    nav = Navegador()
    NAVEGADORES[rol] = nav
    codigo, url, _ = nav.ingresar(nombre, clave)
    revisar(codigo == 200 and "/ingresar" not in url, f"«{rol}» ingresa por el formulario", url)

    _, _, inicio = nav.ver("/")
    es_admin = rol == "administrador"
    revisar(('href="admin/usuarios"' in inicio) == es_admin,
            f"menú dinámico: «Administración» {'SÍ' if es_admin else 'NO'} aparece para «{rol}»")
    revisar(rol in visible(inicio), f"el menú muestra el rol de la sesión ({rol})")

    _, _, aliados = nav.ver("/aliados")
    puede_crear = rol in ("administrador", "editor")
    revisar((">Agregar</button>" in aliados) == puede_crear,
            f"control de acceso: «Agregar» {'SÍ' if puede_crear else 'NO'} aparece para «{rol}»")

    _, _, panel = nav.ver("/admin/usuarios")
    if es_admin:
        revisar("Su rol no permite" not in visible(panel) and "Usuarios" in visible(panel),
                "el administrador SÍ ve el panel de usuarios")
    else:
        revisar("Su rol no permite ver esta pantalla" in visible(panel),
                f"«{rol}» NO ve el panel de usuarios (aviso de rol)")

print("\n-- cierre de sesión")
nav = NAVEGADORES["consulta"]
cookie_vieja = nav.cookie_sesion()
_, _, inicio = nav.ver("/")
codigo, url, _ = nav.ver("/sesion/salir",
                         urllib.parse.urlencode({"__RequestVerificationToken": nav.token_formulario(inicio)}).encode())
revisar("/ingresar" in url, "«Cerrar sesión» lleva a /ingresar", url)

# La prueba fuerte: alguien que se hubiera quedado con la cookie de antes.
peticion = urllib.request.Request(FRONT + "/aliados", headers={"Cookie": f".Innovacion.Sesion={cookie_vieja}"})
try:
    urllib.request.build_opener(_SinRedireccion).open(peticion, timeout=15)
    revisar(False, "la cookie vieja ya no abre /aliados", "respondió 200")
except urllib.error.HTTPError as e:
    revisar(e.code == 302 and "/ingresar" in e.headers.get("Location", ""),
            "la cookie de ANTES del logout ya no abre nada: la sesión y su JWT se borraron del servidor",
            f"{e.code} {e.headers.get('Location', '')}")

print("\n-- limpieza")
for rol, (uid, _, _) in USUARIOS.items():
    esperar(200, "DELETE", f"/api/rol_usuario/{uid}/{ROL[rol]}")
    esperar(200, "DELETE", f"/api/usuario/{uid}")

terminar("Humo v3 · front por rol")
