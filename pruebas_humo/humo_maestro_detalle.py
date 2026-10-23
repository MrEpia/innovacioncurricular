"""Prueba de humo del MAESTRO-DETALLE contra la API real y el procedimiento real.

  POST /api/registro_calificado/completo   y  GET /api/registro_calificado/{codigo}/completo
  POST /api/programa/completo              y  GET /api/programa/{id}/completo

Arma sus propios padres con llaves aleatorias, comprueba los criterios de
v3_maestro_detalle/2_spec.md —incluido el TODO O NADA, llamando al
procedimiento directamente con un hijo malo— y retira todo al final.

Uso:  python pruebas_humo/humo_maestro_detalle.py     (parado en la raíz del proyecto)
"""
import subprocess

from comun import esperar, leer_env, llave_aleatoria, revisar, terminar


def sql(consulta):
    """Ejecuta SQL en el contenedor de SQL Server con la clave del .env y
    devuelve (código de salida, salida)."""
    salida = subprocess.run(
        ["docker", "exec", "innovacion-sqlserver", "/opt/mssql-tools18/bin/sqlcmd",
         "-S", "localhost", "-U", "sa", "-P", leer_env("MSSQL_SA_PASSWORD"), "-C", "-d", "innovacion_local",
         "-h", "-1", "-W", "-b", "-Q", "SET NOCOUNT ON; " + consulta],
        capture_output=True, text=True, encoding="utf-8", errors="replace")
    return salida.returncode, (salida.stdout + salida.stderr).strip()


print("=== Humo · maestro-detalle ===")

F, P0, P, R = llave_aleatoria(6), llave_aleatoria(5), llave_aleatoria(5), llave_aleatoria(3)
ACT1, ACT2, EN1, EN2, PE, CI = (llave_aleatoria(8), llave_aleatoria(8), llave_aleatoria(1),
                                llave_aleatoria(1), llave_aleatoria(1), llave_aleatoria(1))

print("\n-- los padres y los catálogos del detalle")
esperar(201, "POST", "/api/facultad", {"id": F, "nombre": "Facultad humo MD", "tipo": "Facultad",
                                       "fechaFun": "1990-01-01", "universidad": 1})
PROGRAMA = {"nombre": "Programa humo MD", "tipo": "Pregrado", "nivel": "Profesional",
            "fechaCreacion": "2000", "numeroCohortes": "10", "cantGraduados": "100",
            "fechaActualizacion": "2024", "ciudad": "Medellín", "facultad": F}
esperar(201, "POST", "/api/programa", {**PROGRAMA, "id": P0})
for act in (ACT1, ACT2):
    esperar(201, "POST", "/api/activ_academica",
            {"id": act, "nombre": f"Curso {act}", "numCreditos": 3, "tipo": "Obligatoria",
             "areaFormacion": "Básica", "hAcom": 48, "hIndep": 96, "idioma": "Español",
             "espejo": 0, "entidadEspejo": "N/A", "paisEspejo": "N/A", "disenio": P0})
for en in (EN1, EN2):
    esperar(200, "POST", "/api/enfoque", {"id": en, "nombre": f"Enfoque {en}", "descripcion": "Humo"})
esperar(200, "POST", "/api/practica_estrategia", {"id": PE, "tipo": "Práctica", "nombre": "ABP", "descripcion": "Proyectos"})
esperar(200, "POST", "/api/car_innovacion", {"id": CI, "nombre": "Flexibilidad", "descripcion": "Electivas", "tipo": "Curricular"})

# ------------------------------------------------------------------ registro
print("\n-- registro_calificado con su detalle")
REGISTRO = {"codigo": R, "cantCreditos": "160", "horaAcom": "48", "horaInd": "96",
            "metodologia": "Presencial", "fechaInicio": "2024-01-01", "fechaFin": "2031-01-01",
            "duracionAnios": "5", "duracionSemestres": "10", "tipoTitulacion": "Profesional",
            "programa": P0,
            "enfoques": [EN1, EN2],
            "actividades": [{"activAcademicasIdcurso": ACT1, "componente": "Básico", "semestre": "1"},
                            {"activAcademicasIdcurso": ACT2, "componente": "Profesional", "semestre": "3"}]}

esperar(422, "POST", "/api/registro_calificado/completo",
        {**REGISTRO, "actividades": [{"activAcademicasIdcurso": ACT1}]},
        "una actividad sin componente ni semestre → 422")
esperar(400, "POST", "/api/registro_calificado/completo", {**REGISTRO, "enfoques": [], "actividades": []},
        "sin ningún hijo → 400")
esperar(400, "POST", "/api/registro_calificado/completo", {**REGISTRO, "enfoques": [EN1, EN1]},
        "un enfoque repetido → 400")
esperar(400, "POST", "/api/registro_calificado/completo",
        {**REGISTRO, "actividades": REGISTRO["actividades"] + [{"activAcademicasIdcurso": 999999,
                                                                 "componente": "X", "semestre": "9"}]},
        "la TERCERA actividad no existe → 400")
esperar(404, "GET", f"/api/registro_calificado/{R}", descripcion="…y el registro NO se creó → 404")

creado = esperar(201, "POST", "/api/registro_calificado/completo", REGISTRO, "crear con 2 enfoques y 2 actividades → 201")
revisar(creado and creado.get("llave", {}).get("codigo") == R
        and creado.get("detalle") == {"enfoques": 2, "actividades": 2},
        "el 201 trae la llave y cuántos hijos guardó", creado)
esperar(409, "POST", "/api/registro_calificado/completo", REGISTRO, "llave repetida → 409")

ficha = esperar(200, "GET", f"/api/registro_calificado/{R}/completo", descripcion="leer el registro completo → 200")
revisar(ficha and sorted(e["enfoque"] for e in ficha["enfoques"]) == sorted([EN1, EN2])
        and len(ficha["actividades"]) == 2 and all(e.get("nombre") for e in ficha["enfoques"]),
        "el detalle leído es el guardado, con los nombres", ficha)
esperar(200, "GET", f"/api/enfoque_rc/{EN1}/{R}", descripcion="cada hijo existe también en su tabla (enfoque_rc)")
esperar(200, "GET", f"/api/aa_rc/{ACT2}/{R}", descripcion="…y en aa_rc")
esperar(404, "GET", "/api/registro_calificado/999999/completo", descripcion="registro inexistente → 404")

# ------------------------------------------------------------------ programa
print("\n-- programa con su detalle")
COMPLETO = {**PROGRAMA, "id": P, "areasConocimiento": ["1A01", "1A02"],
            "practicasEstrategias": [PE], "caracteristicasInnovacion": [CI]}
esperar(400, "POST", "/api/programa/completo", {**COMPLETO, "facultad": 999999}, "facultad inexistente → 400")
esperar(400, "POST", "/api/programa/completo", {**COMPLETO, "caracteristicasInnovacion": [CI, 999999]},
        "una característica inexistente → 400")
esperar(404, "GET", f"/api/programa/{P}", descripcion="…y el programa NO se creó → 404")
creado = esperar(201, "POST", "/api/programa/completo", COMPLETO, "crear con 2 áreas, 1 práctica y 1 característica → 201")
revisar(creado and creado.get("detalle") == {"areasConocimiento": 2, "practicasEstrategias": 1,
                                             "caracteristicasInnovacion": 1},
        "el 201 cuenta los hijos", creado)
ficha = esperar(200, "GET", f"/api/programa/{P}/completo", descripcion="leer el programa completo → 200")
revisar(ficha and [a["areaConocimiento"] for a in ficha["areasConocimiento"]] == ["1A01", "1A02"]
        and ficha["practicasEstrategias"][0]["practicaEstrategia"] == PE
        and ficha["caracteristicasInnovacion"][0]["carInnovacion"] == CI,
        "el detalle del programa es el guardado", ficha)

# ------------------------------------------------------------------ todo o nada
print("\n-- TODO O NADA dentro del procedimiento (sin pasar por el servicio)")
R2 = R + 1
codigo, salida = sql(
    f"EXEC dbo.sp_registro_calificado_completo_crear @Codigo={R2}, @CantCreditos='1', @HoraAcom='1', "
    f"@HoraInd='1', @Metodologia='X', @FechaInicio='2024-01-01', @FechaFin='2025-01-01', "
    f"@DuracionAnios='1', @DuracionSemestres='2', @TipoTitulacion='X', @Programa={P0}, "
    f"@Enfoques=N'[{EN1}]', @Actividades=N'[{{\"id\":{ACT1},\"componente\":\"A\",\"semestre\":\"1\"}},"
    f"{{\"id\":999999,\"componente\":\"B\",\"semestre\":\"2\"}}]'")
revisar(codigo != 0 and "50400" in salida, "el procedimiento rechaza la segunda actividad (THROW 50400)", salida)
_, quedo = sql(f"SELECT (SELECT COUNT(*) FROM registro_calificado WHERE codigo={R2}) + "
               f"(SELECT COUNT(*) FROM enfoque_rc WHERE registro_calificado={R2}) + "
               f"(SELECT COUNT(*) FROM aa_rc WHERE registro_calificado_codigo={R2})")
revisar(quedo == "0", "…y no quedó NADA: ni el maestro, ni el enfoque, ni la primera actividad", quedo)

# ------------------------------------------------------------------ permisos
print("\n-- permisos (el filtro global también cubre estas rutas)")
esperar(401, "POST", "/api/programa/completo", COMPLETO, "sin token → 401", token=None)

# ------------------------------------------------------------------ limpieza
print("\n-- retiro en orden, de los hijos a los padres")
for ruta in [f"/api/enfoque_rc/{EN1}/{R}", f"/api/enfoque_rc/{EN2}/{R}",
             f"/api/aa_rc/{ACT1}/{R}", f"/api/aa_rc/{ACT2}/{R}", f"/api/registro_calificado/{R}",
             f"/api/programa_ac/{P}/1A01", f"/api/programa_ac/{P}/1A02", f"/api/programa_pe/{P}/{PE}",
             f"/api/programa_ci/{P}/{CI}", f"/api/programa/{P}",
             f"/api/activ_academica/{ACT1}", f"/api/activ_academica/{ACT2}", f"/api/programa/{P0}",
             f"/api/facultad/{F}", f"/api/enfoque/{EN1}", f"/api/enfoque/{EN2}",
             f"/api/practica_estrategia/{PE}", f"/api/car_innovacion/{CI}"]:
    esperar(200, "DELETE", ruta)

terminar("Humo · maestro-detalle")
