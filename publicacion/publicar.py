"""PUBLICACIÓN de la entrega 4 en un hosting ASP.NET gratuito (MonsterASP.NET).

Solo necesita Docker Desktop y Python 3: compila con la imagen oficial del SDK
de .NET y habla con la base del hosting con el sqlcmd de la imagen de SQL
Server, igual que el resto del proyecto.

    python publicacion/publicar.py bd       # crea las 25 tablas y corre las migraciones en la base del hosting
    python publicacion/publicar.py api      # deja publicacion/salida/api.zip listo para subir
    python publicacion/publicar.py front    # deja publicacion/salida/front.zip listo para subir

Los datos del hosting y los secretos salen de publicacion/.env.produccion
(copia de .env.produccion.example). Ese archivo y la carpeta salida/ están en
el .gitignore: NINGÚN secreto llega al repositorio (Artículo 7). Los secretos
viajan solo dentro del web.config del .zip, que va directo al servidor.

Opción: --env RUTA para usar otro archivo de variables.
"""
import os
import re
import shutil
import subprocess
import sys
import tempfile
from xml.sax.saxutils import quoteattr

RAIZ = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
SALIDA = os.path.join(RAIZ, "publicacion", "salida")
IMAGEN_SDK = "mcr.microsoft.com/dotnet/sdk:10.0"
IMAGEN_SQL = "mcr.microsoft.com/mssql/server:2022-latest"

OBLIGATORIAS = {
    "bd": ["SQL_SERVIDOR", "SQL_BASE", "SQL_USUARIO", "SQL_CLAVE"],
    "api": ["SQL_SERVIDOR", "SQL_BASE", "SQL_USUARIO", "SQL_CLAVE", "JWT_SECRETO",
            "ADMIN_USUARIO", "ADMIN_CORREO", "ADMIN_CLAVE"],
    "front": ["URL_API"],
}


def leer_env(ruta):
    if not os.path.exists(ruta):
        sys.exit(f"No existe {ruta}. Cópielo de publicacion/.env.produccion.example y llénelo.")
    valores = {}
    for linea in open(ruta, encoding="utf-8"):
        linea = linea.strip()
        if linea and not linea.startswith("#") and "=" in linea:
            llave, valor = linea.split("=", 1)
            valores[llave.strip()] = valor.strip()
    return valores


def exigir(env, tarea):
    faltan = [v for v in OBLIGATORIAS[tarea] if not env.get(v)]
    if faltan:
        sys.exit(f"Faltan en el .env.produccion: {', '.join(faltan)}")
    if tarea == "api" and len(env["JWT_SECRETO"]) < 32:
        sys.exit("JWT_SECRETO debe tener al menos 32 caracteres (la API no arranca con menos).")


def docker(*argumentos, entrada=None):
    r = subprocess.run(["docker", *argumentos], capture_output=True, text=True,
                       encoding="utf-8", errors="replace", input=entrada)
    return r.returncode, (r.stdout + r.stderr)


# ------------------------------------------------------------------ base de datos
def sqlcmd(env, carpeta, archivo=None, consulta=None):
    """Corre sqlcmd contra la base del hosting desde un contenedor desechable."""
    argumentos = ["run", "--rm", "-v", f"{carpeta}:/scripts:ro", "--entrypoint",
                  "/opt/mssql-tools18/bin/sqlcmd", IMAGEN_SQL,
                  "-S", env["SQL_SERVIDOR"], "-d", env["SQL_BASE"], "-U", env["SQL_USUARIO"],
                  "-P", env["SQL_CLAVE"], "-C", "-b", "-h", "-1", "-W"]
    argumentos += ["-i", f"/scripts/{archivo}"] if archivo else ["-Q", consulta]
    return docker(*argumentos)


def publicar_bd(env):
    """Lo mismo que db/init.sh, pero contra la base del hosting: la base ya la
    creó el panel, así que solo se crean las tablas (si no existen) y se
    corren TODAS las migraciones, que son idempotentes."""
    temporal = tempfile.mkdtemp()
    try:
        codigo, salida = sqlcmd(env, temporal, consulta="SET NOCOUNT ON; SELECT COUNT(*) FROM sys.tables WHERE name = 'programa'")
        if codigo != 0:
            sys.exit(f"No pude conectarme a la base del hosting:\n{salida}")

        if salida.strip().splitlines()[-1].strip() == "1":
            print("[bd] Las tablas ya existen: NO se vuelve a correr el script base (borraría los datos).")
        else:
            # El script del curso hace USE innovacion_local; en el hosting la base
            # tiene otro nombre y ya estamos conectados a ella: esa línea sobra.
            texto = open(os.path.join(RAIZ, "db", "innovacion_curricular.sql"), encoding="utf-8").read()
            texto = re.sub(r"(?im)^\s*USE\s+innovacion_local\s*;?\s*$", "", texto)
            open(os.path.join(temporal, "base.sql"), "w", encoding="utf-8").write(texto)
            print("[bd] Creando las 25 tablas y los catálogos…")
            codigo, salida = sqlcmd(env, temporal, archivo="base.sql")
            if codigo != 0:
                sys.exit(f"Falló el script base:\n{salida[-3000:]}")

        migraciones = os.path.join(RAIZ, "db", "migraciones")
        for nombre in sorted(f for f in os.listdir(migraciones) if f.endswith(".sql")):
            shutil.copy(os.path.join(migraciones, nombre), temporal)
            print(f"[bd] Migración: {nombre}")
            codigo, salida = sqlcmd(env, temporal, archivo=nombre)
            if codigo != 0:
                sys.exit(f"Falló {nombre}:\n{salida[-3000:]}")
        print("[bd] Listo. El administrador inicial lo crea la API al arrancar.")
    finally:
        shutil.rmtree(temporal, ignore_errors=True)


# ------------------------------------------------------------------ aplicaciones
def compilar(carpeta, proyecto, destino):
    """dotnet publish -c Release en un contenedor, sobre una COPIA del código
    (así no se mezclan los bin/obj de Linux con los del PC)."""
    if os.path.exists(destino):
        shutil.rmtree(destino)
    os.makedirs(destino)
    print(f"[{proyecto}] Compilando en Release (la primera vez descarga la imagen del SDK)…")
    codigo, salida = docker(
        "run", "--rm", "-v", f"{os.path.join(RAIZ, carpeta)}:/fuente:ro", "-v", f"{destino}:/salida",
        IMAGEN_SDK, "sh", "-c",
        f"cp -r /fuente /tmp/p && cd /tmp/p && rm -rf bin obj pruebas/bin pruebas/obj && "
        f"dotnet publish {proyecto} -c Release -o /salida --nologo")
    if codigo != 0:
        sys.exit(f"Falló la compilación:\n{salida[-4000:]}")


def poner_variables(destino, variables):
    """Escribe las variables de entorno dentro del web.config que genera
    dotnet publish: IIS (el hosting) se las pasa a la aplicación al arrancarla."""
    ruta = os.path.join(destino, "web.config")
    if not os.path.exists(ruta):
        sys.exit("dotnet publish no generó web.config: el proyecto debe ser Microsoft.NET.Sdk.Web.")
    texto = open(ruta, encoding="utf-8").read()
    lineas = "\n".join(f"          <environmentVariable name={quoteattr(k)} value={quoteattr(v)} />"
                       for k, v in variables.items())
    bloque = f"\n        <environmentVariables>\n{lineas}\n        </environmentVariables>\n      "
    texto, n = re.subn(r"(<aspNetCore\b[^>]*?)\s*/>", lambda m: m.group(1) + ">" + bloque + "</aspNetCore>", texto, count=1)
    if n == 0:
        texto, n = re.subn(r"(<aspNetCore\b[^>]*>)", lambda m: m.group(1) + bloque, texto, count=1)
    if n == 0:
        sys.exit("No encontré el elemento <aspNetCore> en web.config.")
    open(ruta, "w", encoding="utf-8").write(texto)


def empacar(destino, nombre):
    zip_final = shutil.make_archive(os.path.join(SALIDA, nombre), "zip", destino)
    print(f"[{nombre}] Listo: {zip_final}")
    print(f"[{nombre}] Súbalo y descomprímalo en la carpeta wwwroot del sitio (Administrador de archivos o FTP).")


def publicar_api(env):
    destino = os.path.join(SALIDA, "api")
    compilar("api_innovacion", "ApiInnovacion.csproj", destino)
    cadena = (f"Server={env['SQL_SERVIDOR']};Database={env['SQL_BASE']};User Id={env['SQL_USUARIO']};"
              f"Password={env['SQL_CLAVE']};TrustServerCertificate=True;")
    poner_variables(destino, {
        "ASPNETCORE_ENVIRONMENT": "Production",
        "ConnectionStrings__SqlServer": cadena,
        "Jwt__Secreto": env["JWT_SECRETO"],
        "Jwt__Minutos": env.get("JWT_MINUTOS", "60"),
        "AdminInicial__Usuario": env["ADMIN_USUARIO"],
        "AdminInicial__Correo": env["ADMIN_CORREO"],
        "AdminInicial__Clave": env["ADMIN_CLAVE"],
    })
    empacar(destino, "api")


def publicar_front(env):
    destino = os.path.join(SALIDA, "front")
    compilar("front_blazor", "FrontInnovacion.csproj", destino)
    poner_variables(destino, {
        "ASPNETCORE_ENVIRONMENT": "Production",
        "UrlApi": env["URL_API"].rstrip("/"),
    })
    empacar(destino, "front")


if __name__ == "__main__":
    argumentos = sys.argv[1:]
    ruta_env = os.path.join(RAIZ, "publicacion", ".env.produccion")
    if "--env" in argumentos:
        i = argumentos.index("--env")
        ruta_env = argumentos[i + 1]
        del argumentos[i:i + 2]
    if len(argumentos) != 1 or argumentos[0] not in OBLIGATORIAS:
        sys.exit(__doc__)
    tarea = argumentos[0]
    env = leer_env(ruta_env)
    exigir(env, tarea)
    os.makedirs(SALIDA, exist_ok=True)
    {"bd": publicar_bd, "api": publicar_api, "front": publicar_front}[tarea](env)
