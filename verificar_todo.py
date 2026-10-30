"""VERIFICACIÓN COMPLETA del sistema — entregas 1, 2 y 3.

Un solo comando que deja el sistema arriba y corre TODAS las pruebas:

  1. Revisa que exista el .env con todas las variables.
  2. Levanta base, API y front (docker compose up -d --build).
  3. Espera a que la API y el front respondan.
  4. Prueba de capas: los servicios SIN base de datos.
  5. Pruebas de humo contra el sistema real: seguridad (v3), académico,
     actividades y puentes (v2) y el front con sesión (v1 + v3).
  6. Imprime un resumen y sale con código 0 si TODO pasó, 1 si algo falló.

Uso (parado en la raíz del proyecto):
    python verificar_todo.py            # usa la base que ya exista
    python verificar_todo.py --limpio   # borra la base y la crea desde cero

No necesita nada instalado aparte de Docker Desktop y Python 3.
"""
import os
import subprocess
import sys
import time
import urllib.request

RAIZ = os.path.dirname(os.path.abspath(__file__))
API = "http://localhost:8072/"
FRONT = "http://localhost:8073/ingresar"
VARIABLES = ["MSSQL_SA_PASSWORD", "JWT_SECRETO", "JWT_MINUTOS",
             "ADMIN_USUARIO", "ADMIN_CORREO", "ADMIN_CLAVE"]
HUMOS = [
    ("Seguridad: login, JWT y roles (v3)", "humo_seguridad.py"),
    ("Bloque Académico (v2)", "humo_academico.py"),
    ("Bloque Actividades (v2)", "humo_actividades.py"),
    ("Bloque Puentes (v2)", "humo_puentes.py"),
    ("Maestro-detalle: registro calificado y programa", "humo_maestro_detalle.py"),
    ("Front con sesión (v1 + v3)", "humo_front.py"),
    ("Front por rol y cierre de sesión (v3)", "humo_roles_front.py"),
]

resultados = []


def titulo(texto):
    print(f"\n{'=' * 70}\n {texto}\n{'=' * 70}", flush=True)


def correr(comando, mostrar=True):
    # PYTHONIOENCODING: en Windows la salida capturada es cp1252 y las
    # pruebas de humo imprimen tildes y flechas.
    r = subprocess.run(comando, cwd=RAIZ, capture_output=True, text=True,
                       encoding="utf-8", errors="replace",
                       env={**os.environ, "PYTHONIOENCODING": "utf-8"})
    if mostrar and r.returncode != 0:
        print((r.stdout + r.stderr)[-3000:])
    return r


def responde(url):
    try:
        with urllib.request.urlopen(url, timeout=5) as r:
            return r.status == 200
    except Exception:
        return False


def esperar(url, nombre, segundos=600):
    print(f"   esperando a que responda {nombre}…", end="", flush=True)
    inicio = time.time()
    while time.time() - inicio < segundos:
        if responde(url):
            print(f" listo ({int(time.time() - inicio)} s)")
            return True
        print(".", end="", flush=True)
        time.sleep(5)
    print(" NO respondió")
    return False


def anotar(nombre, ok, detalle=""):
    resultados.append((nombre, ok, detalle))
    print(f"   {'[OK]   ' if ok else '[FALLO]'} {nombre} {detalle}")


# ---------------------------------------------------------------- 1. .env
titulo("1. Configuración (.env)")
ruta_env = os.path.join(RAIZ, ".env")
if not os.path.exists(ruta_env):
    print("   Falta el .env. Créelo con:  Copy-Item .env.example .env   (y cambie los valores)")
    sys.exit(1)
definidas = {l.split("=", 1)[0] for l in open(ruta_env, encoding="utf-8") if "=" in l and not l.startswith("#")}
faltan = [v for v in VARIABLES if v not in definidas]
anotar("el .env tiene todas las variables", not faltan, f"faltan: {faltan}" if faltan else "")
if faltan:
    sys.exit(1)

# ---------------------------------------------------------------- 2. Docker
titulo("2. Levantar el sistema con Docker")
if correr(["docker", "info"], mostrar=False).returncode != 0:
    print("   Docker no responde. Abra Docker Desktop y espere a que diga «Engine running».")
    sys.exit(1)
if "--limpio" in sys.argv:
    print("   --limpio: borrando la base para crearla desde cero…")
    correr(["docker", "compose", "down", "-v"])
r = correr(["docker", "compose", "up", "-d", "--build"])
anotar("docker compose up -d --build", r.returncode == 0)
if r.returncode != 0:
    sys.exit(1)

# ---------------------------------------------------------------- 3. Esperar
titulo("3. Esperar a que todo responda")
ok_api = esperar(API, "la API")
ok_front = esperar(FRONT, "el front")
anotar("la API responde", ok_api)
anotar("el front responde", ok_front)
if not (ok_api and ok_front):
    print("   Revise:  docker compose logs api-innovacion   y   docker compose logs front-blazor")
    sys.exit(1)
time.sleep(10)   # margen para que la API cree el administrador inicial

# ---------------------------------------------------------------- 4. Capas
titulo("4. Prueba de capas (sin base de datos)")
r = correr(["docker", "compose", "run", "--rm", "--no-deps", "-T", "api-innovacion",
            "dotnet", "run", "--project", "pruebas/PruebaCapas.csproj"], mostrar=False)
ultima = [l for l in r.stdout.splitlines() if "Prueba de capas" in l]
anotar("prueba de capas", r.returncode == 0, ultima[-1].strip() if ultima else "")
if r.returncode != 0:
    print("\n".join(l for l in r.stdout.splitlines() if "[ERROR]" in l)[-3000:])

# ---------------------------------------------------------------- 5. Humo
titulo("5. Pruebas de humo contra el sistema real")
for nombre, guion in HUMOS:
    r = correr([sys.executable, os.path.join("pruebas_humo", guion)], mostrar=False)
    lineas = r.stdout.splitlines()
    resumen = next((l.strip() for l in reversed(lineas) if "===" in l), "")
    anotar(nombre, r.returncode == 0, resumen)
    if r.returncode != 0:
        print("\n".join(l for l in lineas if "ERROR" in l or "FALLO" in l)[-3000:])

# ---------------------------------------------------------------- Resumen
titulo("RESUMEN")
fallidas = [n for n, ok, _ in resultados if not ok]
for nombre, ok, _ in resultados:
    print(f"   {'OK    ' if ok else 'FALLO '} {nombre}")
print()
if fallidas:
    print(f"   {len(fallidas)} verificación(es) fallaron. El sistema NO está listo.")
    sys.exit(1)
print("   TODO FUNCIONA. Abra http://localhost:8073 e ingrese con ADMIN_USUARIO / ADMIN_CLAVE de su .env.")
sys.exit(0)
