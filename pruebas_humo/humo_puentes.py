"""Prueba de humo del bloque PUENTES de la v2 — programa_ac, programa_pe,
programa_ci, an_programa y enfoque_rc — contra la API real.

Responsable: David Epia.

Necesita que el bloque Académico ya esté integrado. Crea por la API un
programa y un registro calificado, y por los endpoints de la v1 una práctica,
una característica, un aspecto normativo y un enfoque; relaciona todo y lo
retira al final, de los hijos hacia los padres.

Uso:  python pruebas_humo/humo_puentes.py     (parado en la raíz del proyecto)
"""
from comun import api, esperar, llave_aleatoria, revisar, terminar

print("=== Humo v2 · bloque Puentes ===")

F, P, R = llave_aleatoria(6), llave_aleatoria(5), llave_aleatoria(3)
PE, CI, AN, EN = llave_aleatoria(1), llave_aleatoria(1), llave_aleatoria(1), llave_aleatoria(1)

print("\n-- los padres")
esperar(201, "POST", "/api/facultad", {"id": F, "nombre": "Facultad humo puentes", "tipo": "Facultad",
                                       "fechaFun": "1990-01-01", "universidad": 1})
esperar(201, "POST", "/api/programa", {"id": P, "nombre": "Programa humo puentes", "tipo": "Pregrado",
                                       "nivel": "Profesional", "fechaCreacion": "2000", "numeroCohortes": "10",
                                       "cantGraduados": "100", "fechaActualizacion": "2024",
                                       "ciudad": "Medellín", "facultad": F})
esperar(201, "POST", "/api/registro_calificado", {"codigo": R, "cantCreditos": "160", "horaAcom": "48",
                                                  "horaInd": "96", "metodologia": "Virtual",
                                                  "fechaInicio": "2022-01-01", "fechaFin": "2029-01-01",
                                                  "duracionAnios": "5", "duracionSemestres": "10",
                                                  "tipoTitulacion": "Profesional", "programa": P})
esperar(200, "POST", "/api/practica_estrategia", {"id": PE, "tipo": "Práctica", "nombre": "ABP", "descripcion": "Proyectos"})
esperar(200, "POST", "/api/car_innovacion", {"id": CI, "nombre": "Flexibilidad", "descripcion": "Electivas", "tipo": "Curricular"})
esperar(200, "POST", "/api/aspecto_normativo", {"id": AN, "tipo": "Decreto", "descripcion": "Registro calificado", "fuente": "MEN"})
esperar(200, "POST", "/api/enfoque", {"id": EN, "nombre": "Constructivista", "descripcion": "Aprender haciendo"})

RELACIONES = [
    ("programa_ac", {"programa": P, "areaConocimiento": "1A01"}, f"{P}/1A01", "area_conocimiento", "1A01"),
    ("programa_pe", {"programa": P, "practicaEstrategia": PE}, f"{P}/{PE}", "practica_estrategia", PE),
    ("programa_ci", {"programa": P, "carInnovacion": CI}, f"{P}/{CI}", "car_innovacion", CI),
    ("an_programa", {"aspectoNormativo": AN, "programa": P}, f"{AN}/{P}", "aspecto_normativo", AN),
    ("enfoque_rc", {"enfoque": EN, "registroCalificado": R}, f"{EN}/{R}", "enfoque", EN),
]

for tabla, cuerpo, llave, padre_v1, llave_padre in RELACIONES:
    print(f"\n-- {tabla}")
    malo = {k: (999999 if isinstance(v, int) else "ZZ99") for k, v in cuerpo.items()}
    esperar(400, "POST", f"/api/{tabla}", malo, "padres inexistentes → 400")
    esperar(201, "POST", f"/api/{tabla}", cuerpo, "crear la relación → 201")
    esperar(409, "POST", f"/api/{tabla}", cuerpo, "relación repetida → 409")
    esperar(200, "GET", f"/api/{tabla}/{llave}", descripcion="obtener por llave compuesta → 200")
    codigo, _ = api("PUT", f"/api/{tabla}/{llave}", cuerpo)
    revisar(codigo in (404, 405), "una puente pura no tiene PUT (C5)", codigo)
    esperar(409, "DELETE", f"/api/{padre_v1}/{llave_padre}",
            descripcion=f"el padre {padre_v1} (v1) con la relación activa → 409 (RF9)")

print("\n-- retiro en orden")
for tabla, _, llave, _, _ in RELACIONES:
    esperar(200, "DELETE", f"/api/{tabla}/{llave}")
    esperar(404, "DELETE", f"/api/{tabla}/{llave}", descripcion=f"segundo retiro de {tabla} → 404")
for ruta in [f"/api/registro_calificado/{R}", f"/api/programa/{P}", f"/api/facultad/{F}",
             f"/api/practica_estrategia/{PE}", f"/api/car_innovacion/{CI}",
             f"/api/aspecto_normativo/{AN}", f"/api/enfoque/{EN}"]:
    esperar(200, "DELETE", ruta)
esperar(200, "GET", "/api/area_conocimiento/1A01", descripcion="el área 1A01 del catálogo sigue intacta")

terminar("Humo v2 · Puentes")
