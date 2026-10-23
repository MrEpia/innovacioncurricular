"""Prueba de humo del bloque ACTIVIDADES de la v2 — activ_academica, pasantia,
premio, docente_departamento, alianza y aa_rc — contra la API real.

Responsable: David Epia.

Necesita que el bloque Académico ya esté integrado: construye su cadena por la
API (facultad → programa → registro calificado) con llaves aleatorias, crea un
aliado de la v1, comprueba los criterios 2 a 8 de la spec de la v2 y retira
todo al final, de los hijos hacia los padres.

Uso:  python pruebas_humo/humo_actividades.py     (parado en la raíz del proyecto)
"""
from comun import api, esperar, llave_aleatoria, revisar, terminar

print("=== Humo v2 · bloque Actividades ===")

F, P, R, ACT, PAS, PRE, DOC, NIT = (llave_aleatoria(6), llave_aleatoria(5), llave_aleatoria(3),
                                    llave_aleatoria(2), llave_aleatoria(2), llave_aleatoria(2),
                                    llave_aleatoria(1), llave_aleatoria(8))

print("\n-- la cadena de padres (bloque Académico y v1)")
esperar(201, "POST", "/api/facultad", {"id": F, "nombre": "Facultad humo act", "tipo": "Facultad",
                                       "fechaFun": "1990-01-01", "universidad": 1})
esperar(201, "POST", "/api/programa", {"id": P, "nombre": "Programa humo act", "tipo": "Pregrado",
                                       "nivel": "Profesional", "fechaCreacion": "2000", "numeroCohortes": "10",
                                       "cantGraduados": "100", "fechaActualizacion": "2024",
                                       "ciudad": "Cali", "facultad": F})
esperar(201, "POST", "/api/registro_calificado", {"codigo": R, "cantCreditos": "160", "horaAcom": "48",
                                                  "horaInd": "96", "metodologia": "Presencial",
                                                  "fechaInicio": "2022-01-01", "fechaFin": "2029-01-01",
                                                  "duracionAnios": "5", "duracionSemestres": "10",
                                                  "tipoTitulacion": "Profesional", "programa": P})
esperar(200, "POST", "/api/aliado", {"nit": NIT, "razonSocial": "Aliado humo", "nombreContacto": "Ana",
                                     "correo": "ana@aliado.co", "telefono": "300", "ciudad": "Cali"},
        "crear aliado (v1 responde 200)")

print("\n-- activ_academica")
ACTIVIDAD = {"id": ACT, "nombre": "Cálculo I", "numCreditos": 4, "tipo": "Obligatoria",
             "areaFormacion": "Básica", "hAcom": 64, "hIndep": 128, "idioma": "Español",
             "espejo": 0, "entidadEspejo": "N/A", "paisEspejo": "N/A", "disenio": P}
esperar(400, "POST", "/api/activ_academica", {**ACTIVIDAD, "numCreditos": -1}, "créditos negativos → 400")
esperar(400, "POST", "/api/activ_academica", {**ACTIVIDAD, "espejo": 5}, "espejo distinto de 0/1 → 400")
esperar(400, "POST", "/api/activ_academica", {**ACTIVIDAD, "disenio": 999999}, "programa de diseño inexistente → 400")
esperar(201, "POST", "/api/activ_academica", {**ACTIVIDAD, "id": ACT + 1, "disenio": None},
        "sin programa de diseño (FK opcional) → 201")
esperar(201, "POST", "/api/activ_academica", ACTIVIDAD, "crear actividad → 201")
esperar(409, "POST", "/api/activ_academica", ACTIVIDAD, "llave repetida → 409")
esperar(422, "PUT", f"/api/activ_academica/{ACT}", {"nombre": "Cálculo diferencial"}, "PUT incompleto → 422")
esperar(200, "PATCH", f"/api/activ_academica/{ACT}", {"nombre": "Cálculo diferencial"}, "PATCH con el mismo cuerpo → 200")

print("\n-- pasantia y premio")
esperar(400, "POST", "/api/pasantia", {"id": PAS, "nombre": "Pasantía", "pais": "Colombia", "empresa": "Empresa",
                                       "descripcion": "Práctica", "programa": 999999}, "programa inexistente → 400")
esperar(201, "POST", "/api/pasantia", {"id": PAS, "nombre": "Pasantía", "pais": "Colombia", "empresa": "Empresa",
                                       "descripcion": "Práctica", "programa": P})
esperar(201, "POST", "/api/premio", {"id": PRE, "nombre": "Premio", "descripcion": "Reconocimiento",
                                     "fecha": "2023-11-20", "entidadOtorgante": "MinEducación",
                                     "pais": "Colombia", "programa": P})

print("\n-- docente_departamento (llave compuesta)")
VINC = {"docente": DOC, "departamento": P, "dedicacion": "Tiempo Completo", "modalidad": "Planta",
        "fechaIngreso": "2014-02-01"}
esperar(400, "POST", "/api/docente_departamento", {**VINC, "dedicacion": "Ocasional"}, "dedicación fuera de la lista → 400")
esperar(400, "POST", "/api/docente_departamento", {**VINC, "fechaSalida": "2010-01-01"}, "salida antes del ingreso → 400")
esperar(201, "POST", "/api/docente_departamento", VINC, "crear vinculación → 201")
esperar(200, "GET", f"/api/docente_departamento/{DOC}/{P}", descripcion="obtener por llave compuesta → 200")
esperar(200, "PATCH", f"/api/docente_departamento/{DOC}/{P}", {"fechaSalida": "2015-12-31"},
        "PATCH de la fecha de salida → 200")
esperar(404, "GET", f"/api/docente_departamento/{DOC}/999999", descripcion="llave compuesta inexistente → 404")

print("\n-- alianza")
esperar(400, "POST", "/api/alianza", {"aliado": 999999, "departamento": P, "fechaInicio": "2023-01-01"},
        "aliado inexistente → 400")
esperar(201, "POST", "/api/alianza", {"aliado": NIT, "departamento": P, "fechaInicio": "2023-01-01",
                                      "docente": DOC}, "crear alianza → 201")
esperar(409, "DELETE", f"/api/aliado/{NIT}", descripcion="aliado (v1) con alianza activa → 409 (RF9)")

print("\n-- aa_rc")
esperar(400, "POST", "/api/aa_rc", {"activAcademicasIdcurso": 999999, "registroCalificadoCodigo": R,
                                    "componente": "Básico", "semestre": "1"}, "actividad inexistente → 400")
esperar(201, "POST", "/api/aa_rc", {"activAcademicasIdcurso": ACT, "registroCalificadoCodigo": R,
                                    "componente": "Básico", "semestre": "1"}, "crear aa_rc → 201")
esperar(200, "PATCH", f"/api/aa_rc/{ACT}/{R}", {"semestre": "2"}, "PATCH del semestre → 200")
esperar(409, "DELETE", f"/api/activ_academica/{ACT}", descripcion="actividad con aa_rc activo → 409")
esperar(409, "DELETE", f"/api/programa/{P}", descripcion="programa con actividades, pasantías… → 409")

print("\n-- retiro en orden")
for ruta in [f"/api/aa_rc/{ACT}/{R}", f"/api/alianza/{NIT}/{P}", f"/api/docente_departamento/{DOC}/{P}",
             f"/api/premio/{PRE}", f"/api/pasantia/{PAS}", f"/api/activ_academica/{ACT}",
             f"/api/activ_academica/{ACT + 1}", f"/api/registro_calificado/{R}", f"/api/programa/{P}",
             f"/api/facultad/{F}", f"/api/aliado/{NIT}"]:
    esperar(200, "DELETE", ruta)
esperar(404, "GET", f"/api/alianza/{NIT}/{P}", descripcion="la alianza retirada ya no se encuentra → 404")

terminar("Humo v2 · Actividades")
