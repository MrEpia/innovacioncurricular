namespace ApiInnovacion.Modelos;

// ============================================================================
// v4 — Las filas de las 10 CONSULTAS MULTITABLA. Una clase por reporte: cada
// uno dice qué columnas devuelve, y Swagger las documenta (Artículo 10.1:
// nada genérico). Son de solo lectura: no hay POST, PUT ni DELETE de reportes.
// ============================================================================

/// <summary>R1 — Programas acreditados (acreditacion · programa · facultad · universidad).</summary>
public class ProgramaAcreditado
{
    public string Universidad { get; set; } = string.Empty;
    public string Ciudad { get; set; } = string.Empty;
    public string Facultad { get; set; } = string.Empty;
    public string Programa { get; set; } = string.Empty;
    public string Nivel { get; set; } = string.Empty;
    public int Resolucion { get; set; }
    public string TipoAcreditacion { get; set; } = string.Empty;
    public string Calificacion { get; set; } = string.Empty;
    public string Desde { get; set; } = string.Empty;
    public string Hasta { get; set; } = string.Empty;
}

/// <summary>R2 — Registros calificados con sus enfoques (facultad · programa · registro_calificado · enfoque_rc · enfoque).</summary>
public class RegistroConEnfoques
{
    public string Facultad { get; set; } = string.Empty;
    public string Programa { get; set; } = string.Empty;
    public int Registro { get; set; }
    public string Metodologia { get; set; } = string.Empty;
    public DateTime VigenteHasta { get; set; }
    public int CantidadEnfoques { get; set; }
    public string Enfoques { get; set; } = string.Empty;
}

/// <summary>R3 — Plan de estudios (programa · registro_calificado · aa_rc · activ_academica).</summary>
public class ActividadDelPlan
{
    public string Programa { get; set; } = string.Empty;
    public int Registro { get; set; }
    public string Actividad { get; set; } = string.Empty;
    public int Creditos { get; set; }
    public string Componente { get; set; } = string.Empty;
    public string Semestre { get; set; } = string.Empty;
    public string Idioma { get; set; } = string.Empty;
}

/// <summary>R4 — Áreas de conocimiento por facultad (facultad · programa · programa_ac · area_conocimiento).</summary>
public class AreasDeFacultad
{
    public string Facultad { get; set; } = string.Empty;
    public string GranArea { get; set; } = string.Empty;
    public int Programas { get; set; }
    public int Areas { get; set; }
}

/// <summary>R5 — Innovación por programa (programa · facultad · programa_ci · car_innovacion · programa_pe · practica_estrategia).</summary>
public class InnovacionDePrograma
{
    public string Facultad { get; set; } = string.Empty;
    public string Programa { get; set; } = string.Empty;
    public string Caracteristicas { get; set; } = string.Empty;
    public string Practicas { get; set; } = string.Empty;
}

/// <summary>R6 — Alianzas vigentes (aliado · alianza · programa · facultad · universidad).</summary>
public class AlianzaVigente
{
    public string Aliado { get; set; } = string.Empty;
    public string CiudadAliado { get; set; } = string.Empty;
    public string Programa { get; set; } = string.Empty;
    public string Facultad { get; set; } = string.Empty;
    public string Sede { get; set; } = string.Empty;
    public DateTime Desde { get; set; }
    public DateTime? Hasta { get; set; }
    public int? Docente { get; set; }
}

/// <summary>R7 — Normatividad por programa (aspecto_normativo · an_programa · programa · facultad).</summary>
public class NormaDePrograma
{
    public string Norma { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public string Fuente { get; set; } = string.Empty;
    public string Programa { get; set; } = string.Empty;
    public string Facultad { get; set; } = string.Empty;
}

/// <summary>R8 — Internacionalización y reconocimientos (programa · facultad · pasantia · premio).</summary>
public class InternacionalizacionDePrograma
{
    public string Facultad { get; set; } = string.Empty;
    public string Programa { get; set; } = string.Empty;
    public int Pasantias { get; set; }
    public string PaisesPasantia { get; set; } = string.Empty;
    public int Premios { get; set; }
    public string UltimoPremio { get; set; } = string.Empty;
}

/// <summary>R9 — Docentes por sede, facultad y dedicación (universidad · facultad · programa · docente_departamento).</summary>
public class DocentesDeSede
{
    public string Sede { get; set; } = string.Empty;
    public string Facultad { get; set; } = string.Empty;
    public string Dedicacion { get; set; } = string.Empty;
    public int Docentes { get; set; }
}

/// <summary>R10 — Cursos espejo internacionales (activ_academica · programa · facultad · universidad).</summary>
public class CursoEspejo
{
    public string Actividad { get; set; } = string.Empty;
    public int Creditos { get; set; }
    public string Idioma { get; set; } = string.Empty;
    public string EntidadEspejo { get; set; } = string.Empty;
    public string PaisEspejo { get; set; } = string.Empty;
    public string Programa { get; set; } = string.Empty;
    public string Facultad { get; set; } = string.Empty;
    public string Sede { get; set; } = string.Empty;
}

// ============================================================================
// v4 — DASHBOARD: datos AGREGADOS, listos para graficar.
// ============================================================================

/// <summary>Las cifras de las tarjetas del dashboard (solo registros activos).</summary>
public class ResumenDashboard
{
    public int Facultades { get; set; }
    public int Programas { get; set; }
    public int Acreditaciones { get; set; }
    public int RegistrosCalificados { get; set; }
    public int ActividadesAcademicas { get; set; }
    public int Docentes { get; set; }
    public int Alianzas { get; set; }
    public int Pasantias { get; set; }
    public int Premios { get; set; }
    public int Graduados { get; set; }
}

/// <summary>Una barra o una porción de un gráfico: su etiqueta y su valor.</summary>
public class DatoGrafico
{
    public string Etiqueta { get; set; } = string.Empty;
    public int Valor { get; set; }
}
