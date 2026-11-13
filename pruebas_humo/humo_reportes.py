"""Prueba de humo de la ENTREGA 4 contra el sistema levantado.

  API:  las 10 consultas multitabla (/api/reportes/*) y los datos agregados
        del dashboard (/api/dashboard/*).
  Front: las 5 páginas corporativas públicas, la PWA (manifiesto, service
        worker, página sin conexión) y que reportes y dashboard pidan sesión.

Necesita los datos de demostración de db/migraciones/v4_001_datos_demo.sql,
que db/init.sh carga solo. Lo que crea para probar el filtro de activos lo
retira al final.

Uso:  python pruebas_humo/humo_reportes.py     (parado en la raíz del proyecto)
"""
import json
import os
import urllib.error
import urllib.request

from comun import api, esperar, llave_aleatoria, revisar, terminar

FRONT = os.environ.get("FRONT_URL", "http://localhost:8073")

COLUMNAS = {
    "programas-acreditados": {"universidad", "facultad", "programa", "resolucion", "calificacion"},
    "registros-con-enfoques": {"programa", "registro", "enfoques", "cantidadEnfoques"},
    "plan-de-estudios": {"programa", "registro", "actividad", "componente", "semestre"},
    "areas-por-facultad": {"facultad", "granArea", "programas", "areas"},
    "innovacion-por-programa": {"programa", "caracteristicas", "practicas"},
    "alianzas-vigentes": {"aliado", "programa", "sede", "desde"},
    "normatividad-por-programa": {"norma", "fuente", "programa", "facultad"},
    "internacionalizacion": {"programa", "pasantias", "premios"},
    "docentes-por-sede": {"sede", "facultad", "dedicacion", "docentes"},
    "cursos-espejo": {"actividad", "paisEspejo", "programa", "sede"},
}


def front(ruta):
    """GET al front SIN sesión y sin seguir redirecciones: (código, Location)."""
    class SinRedireccion(urllib.request.HTTPRedirectHandler):
        def redirect_request(self, *a, **k):
            return None
    try:
        with urllib.request.build_opener(SinRedireccion).open(FRONT + ruta, timeout=20) as r:
            return r.status, r.read().decode("utf-8", "replace")
    except urllib.error.HTTPError as e:
        return e.code, e.headers.get("Location", "")


print("=== Humo v4 · reportes, dashboard, páginas corporativas y PWA ===")

print("\n-- el catálogo de reportes")
catalogo = esperar(200, "GET", "/api/reportes", descripcion="GET /api/reportes → 200")
fichas = (catalogo or {}).get("datos", [])
revisar(len(fichas) == 10, "el catálogo trae 10 reportes", len(fichas))
revisar(all(len(f["tablas"]) >= 4 for f in fichas), "todos unen 4 tablas o más",
        [f["ruta"] for f in fichas if len(f["tablas"]) < 4])

print("\n-- las 10 consultas multitabla (con los datos de demostración)")
for ficha in fichas:
    ruta = ficha["ruta"]
    cuerpo = esperar(200, "GET", f"/api/reportes/{ruta}", descripcion=f"{ruta:28s} → 200")
    datos = (cuerpo or {}).get("datos", [])
    revisar(len(datos) > 0 and cuerpo.get("total") == len(datos), f"{ruta:28s} trae filas y su total cuadra",
            cuerpo and cuerpo.get("total"))
    faltan = COLUMNAS.get(ruta, set()) - set(datos[0].keys() if datos else [])
    revisar(not faltan, f"{ruta:28s} trae sus columnas", faltan)

print("\n-- un registro RETIRADO sale de los reportes")
R = llave_aleatoria(3)
esperar(201, "POST", "/api/acreditacion",
        {"resolucion": R, "tipo": "Alta calidad", "calificacion": "Excelente",
         "fechaInicio": "2025", "fechaFin": "2031", "programa": 201}, "crear una acreditación de prueba → 201")
_, cuerpo = api("GET", "/api/reportes/programas-acreditados")
revisar(any(f["resolucion"] == R for f in (cuerpo or {}).get("datos", [])), "aparece en «programas acreditados»")
esperar(200, "DELETE", f"/api/acreditacion/{R}", descripcion="retirarla → 200")
_, cuerpo = api("GET", "/api/reportes/programas-acreditados")
revisar(not any(f["resolucion"] == R for f in (cuerpo or {}).get("datos", [])), "y ya no aparece: el reporte filtra activo = 1")

print("\n-- el dashboard (datos agregados)")
resumen = esperar(200, "GET", "/api/dashboard/resumen", descripcion="resumen → 200")
_, programas = api("GET", "/api/programa?limite=1000")
revisar(resumen and programas and resumen["programas"] == programas["total"],
        "el resumen cuenta los mismos programas activos que GET /api/programa",
        (resumen or {}).get("programas"))
for grafico in ("programas-por-nivel", "programas-por-sede", "acreditaciones-por-calificacion",
                "registros-por-metodologia", "docentes-por-dedicacion", "graduados-por-facultad"):
    serie = esperar(200, "GET", f"/api/dashboard/{grafico}", descripcion=f"{grafico:32s} → 200")
    datos = (serie or {}).get("datos", [])
    revisar(datos and all({"etiqueta", "valor"} <= set(d) for d in datos)
            and serie["total"] == sum(d["valor"] for d in datos),
            f"{grafico:32s} trae {{etiqueta, valor}} y su total cuadra")
_, nivel = api("GET", "/api/dashboard/programas-por-nivel")
revisar(nivel and resumen and nivel["total"] == resumen["programas"], "programas por nivel suma el total de programas")
corto = esperar(200, "GET", "/api/dashboard/graduados-por-facultad?maximo=2", descripcion="graduados con maximo=2 → 200")
revisar(corto and len(corto["datos"]) <= 3, "con maximo=2 hay a lo sumo 3 barras (2 + «Otras»)")
esperar(400, "GET", "/api/dashboard/graduados-por-facultad?maximo=0", descripcion="maximo=0 → 400")

print("\n-- seguridad: sin token no hay reportes ni dashboard")
esperar(401, "GET", "/api/reportes/cursos-espejo", descripcion="reporte sin token → 401", token=None)
esperar(401, "GET", "/api/dashboard/resumen", descripcion="dashboard sin token → 401", token=None)

print("\n-- documentación: Swagger describe los endpoints nuevos")
try:
    with urllib.request.urlopen(os.environ.get("API_URL", "http://localhost:8072") + "/swagger/v1/swagger.json", timeout=20) as r:
        swagger = json.loads(r.read())
    rutas = swagger.get("paths", {})
    revisar(sum(1 for p in rutas if p.startswith("/api/reportes/")) == 10, "Swagger documenta los 10 reportes")
    revisar(sum(1 for p in rutas if p.startswith("/api/dashboard/")) == 7, "Swagger documenta los 7 endpoints del dashboard")
    revisar(bool(rutas.get("/api/reportes/cursos-espejo", {}).get("get", {}).get("summary")),
            "y cada uno con su descripción (comentarios ///)")
except Exception as e:  # noqa: BLE001
    revisar(False, "Swagger responde", str(e))

print("\n-- front: páginas corporativas PÚBLICAS")
for ruta, texto in [("/", "Sistema de innovación curricular"), ("/servicios", "Servicios"),
                    ("/soporte", "Preguntas frecuentes"), ("/contacto", "Escríbanos"),
                    ("/nosotros", "Sobre nosotros")]:
    codigo, html = front(ruta)
    revisar(codigo == 200 and texto in html, f"{ruta:11s} responde sin sesión y muestra «{texto}»", codigo)
codigo, html = front("/")
revisar('href="ingresar"' in html, "la página principal ofrece «Ingresar»")
revisar("marca/logo-horizontal.png" in html and "fonts.googleapis.com" in html,
        "imagen corporativa: logo del manual y tipografía Merriweather/Inter")

print("\n-- front: PWA")
codigo, manifiesto = front("/manifest.webmanifest")
try:
    m = json.loads(manifiesto)
    revisar(codigo == 200 and m.get("display") == "standalone" and len(m.get("icons", [])) >= 2,
            "manifest.webmanifest: instalable (standalone, con íconos)")
    revisar(m.get("theme_color", "").upper() == "#1F5E4C", "el color del tema es el verde del manual")
except ValueError:
    revisar(False, "manifest.webmanifest es JSON", manifiesto[:80])
revisar(front("/service-worker.js")[0] == 200, "service-worker.js responde")
revisar(front("/offline.html")[0] == 200, "offline.html (página sin conexión) responde")
revisar('rel="manifest"' in html and "serviceWorker" in html, "la página enlaza el manifiesto y registra el service worker")

print("\n-- front: reportes y dashboard piden sesión")
for ruta in ("/reportes", "/dashboard"):
    codigo, destino = front(ruta)
    revisar(codigo == 302 and "/ingresar" in destino, f"{ruta} sin sesión → /ingresar", f"{codigo} {destino}")

terminar("Humo v4 · reportes y dashboard")
