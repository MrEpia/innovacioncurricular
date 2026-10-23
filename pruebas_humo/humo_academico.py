"""Prueba de humo del bloque ACADÉMICO de la v2 — facultad, programa,
acreditacion y registro_calificado — contra la API real.

Responsable: Sebastián Calvo.

Construye su propia cadena por la API (universidad 1 → facultad → programa →
acreditación y registro calificado) con llaves aleatorias, comprueba los
criterios 2 a 8 de la spec de la v2 y la retira al final, de los hijos hacia
los padres. No depende de datos sembrados, salvo la universidad 1 del Excel.

Uso:  python pruebas_humo/humo_academico.py     (parado en la raíz del proyecto)
"""
import os
import subprocess

from comun import api, esperar, llave_aleatoria, revisar, terminar


def activo_en_base(tabla, columna, llave):
    """Lee la columna `activo` directamente en SQL Server (criterio 8). Usa la
    clave del .env: el guion no la tiene escrita."""
    clave = None
    ruta_env = os.path.join(os.path.dirname(__file__), "..", ".env")
    with open(ruta_env, encoding="utf-8") as f:
        for linea in f:
            if linea.startswith("MSSQL_SA_PASSWORD="):
                clave = linea.strip().split("=", 1)[1]
    salida = subprocess.run(
        ["docker", "exec", "innovacion-sqlserver", "/opt/mssql-tools18/bin/sqlcmd",
         "-S", "localhost", "-U", "sa", "-P", clave, "-C", "-d", "innovacion_local",
         "-h", "-1", "-W", "-Q",
         f"SET NOCOUNT ON; SELECT activo FROM {tabla} WHERE {columna} = {int(llave)}"],
        capture_output=True, text=True)
    return salida.stdout.strip()


print("=== Humo v2 · bloque Académico ===")

_, diag = api("GET", "/")
revisar(diag and diag.get("version") in ("v2", "v3"), "GET / responde la v2 (o posterior)", diag)
esperar(200, "GET", "/api/universidad/1", descripcion="la universidad 1 del Excel existe")

F, F2, P, A, R = (llave_aleatoria(6), llave_aleatoria(6), llave_aleatoria(5),
                  llave_aleatoria(4), llave_aleatoria(3))

FACULTAD = {"id": F, "nombre": "Facultad de humo", "tipo": "Facultad",
            "fechaFun": "1990-03-15", "universidad": 1}

print("\n-- facultad")
esperar(422, "POST", "/api/facultad", {"id": F, "tipo": "Facultad"}, "sin campos obligatorios → 422")
esperar(400, "POST", "/api/facultad", {**FACULTAD, "universidad": 999999}, "universidad inexistente → 400")
esperar(400, "POST", "/api/facultad", {**FACULTAD, "tipo": "Escuela"}, "tipo fuera de la lista → 400")
creada = esperar(201, "POST", "/api/facultad", FACULTAD, "crear facultad → 201")
revisar(creada and creada.get("llave", {}).get("id") == F, "el 201 trae la llave creada", creada)
esperar(409, "POST", "/api/facultad", FACULTAD, "llave repetida → 409")
listado = esperar(200, "GET", "/api/facultad", descripcion="listar facultades → 200")
revisar(listado and any(x["id"] == F for x in listado["datos"]), "la facultad aparece en el listado")

print("\n-- programa")
PROGRAMA = {"id": P, "nombre": "Programa de humo", "tipo": "Pregrado", "nivel": "Profesional",
            "fechaCreacion": "2001", "numeroCohortes": "20", "cantGraduados": "500",
            "fechaActualizacion": "2024", "ciudad": "Bogotá", "facultad": F}
esperar(400, "POST", "/api/programa", {**PROGRAMA, "facultad": 999999}, "facultad inexistente → 400")
esperar(201, "POST", "/api/programa", PROGRAMA, "crear programa sin fecha de cierre (opcional) → 201")
ficha = esperar(200, "GET", f"/api/programa/{P}", descripcion="obtener programa → 200")
revisar(ficha and ficha.get("fechaCierre") is None, "fechaCierre quedó en NULL")

print("\n-- un padre RETIRADO no sirve de padre (D-v2-5)")
esperar(201, "POST", "/api/facultad", {**FACULTAD, "id": F2, "nombre": "Facultad retirada"})
esperar(200, "DELETE", f"/api/facultad/{F2}", descripcion="retirar facultad sin hijos → 200")
esperar(404, "GET", f"/api/facultad/{F2}", descripcion="la facultad retirada ya no se encuentra → 404")
esperar(400, "POST", "/api/programa", {**PROGRAMA, "id": P + 1, "facultad": F2},
        "programa apuntando a una facultad retirada → 400")
esperar(404, "GET", f"/api/programa/{P + 1}", descripcion="…y no se creó nada → 404")

print("\n-- acreditacion: PUT contra PATCH")
ACRED = {"resolucion": A, "tipo": "Alta calidad", "calificacion": "Excelente",
         "fechaInicio": "2020", "fechaFin": "2028", "programa": P}
esperar(201, "POST", "/api/acreditacion", ACRED, "crear acreditación → 201")
esperar(422, "PUT", f"/api/acreditacion/{A}", {"calificacion": "Muy buena"},
        "PUT con cuerpo incompleto → 422")
esperar(200, "PATCH", f"/api/acreditacion/{A}", {"calificacion": "Muy buena"},
        "PATCH con el MISMO cuerpo → 200")
ficha = esperar(200, "GET", f"/api/acreditacion/{A}")
revisar(ficha and ficha["calificacion"] == "Muy buena" and ficha["tipo"] == "Alta calidad",
        "el PATCH cambió solo la calificación", ficha)
esperar(400, "PATCH", f"/api/acreditacion/{A}", {}, "PATCH vacío → 400")
esperar(400, "PATCH", f"/api/acreditacion/{A}", {"programa": 999999}, "PATCH a un programa inexistente → 400")

print("\n-- registro_calificado: reglas de fechas")
REG = {"codigo": R, "cantCreditos": "160", "horaAcom": "48", "horaInd": "96",
       "metodologia": "Presencial", "fechaInicio": "2022-01-01", "fechaFin": "2029-01-01",
       "duracionAnios": "5", "duracionSemestres": "10", "tipoTitulacion": "Profesional", "programa": P}
esperar(400, "POST", "/api/registro_calificado", {**REG, "fechaFin": "2021-01-01"},
        "fecha de fin anterior a la de inicio → 400")
esperar(201, "POST", "/api/registro_calificado", REG, "crear registro calificado → 201")
esperar(400, "PATCH", f"/api/registro_calificado/{R}", {"fechaFin": "2000-01-01"},
        "PATCH con solo fechaFin se valida contra la fecha guardada → 400")

print("\n-- huérfanos: retirar un padre con hijos activos (criterio 4)")
cuerpo = esperar(409, "DELETE", f"/api/programa/{P}", descripcion="programa con acreditación y registro → 409")
revisar(cuerpo and "acreditacion" in cuerpo.get("detalle", ""), "el 409 dice qué tablas dependen", cuerpo)
esperar(409, "DELETE", f"/api/facultad/{F}", descripcion="facultad con programa → 409")
esperar(409, "DELETE", "/api/universidad/1", descripcion="universidad (v1) con facultad → 409 (RF9)")

print("\n-- retiro en orden: de los hijos a los padres")
esperar(200, "DELETE", f"/api/acreditacion/{A}")
esperar(404, "DELETE", f"/api/acreditacion/{A}", descripcion="segundo retiro → 404")
revisar(activo_en_base("acreditacion", "resolucion", A) == "0",
        "la acreditación sigue en la base con activo = 0 (borrado lógico)")
esperar(200, "DELETE", f"/api/registro_calificado/{R}")
esperar(200, "DELETE", f"/api/programa/{P}", descripcion="sin hijos activos, el programa se retira → 200")
esperar(200, "DELETE", f"/api/facultad/{F}")
esperar(200, "GET", "/api/universidad/1", descripcion="la universidad 1 sigue intacta")

terminar("Humo v2 · Académico")
