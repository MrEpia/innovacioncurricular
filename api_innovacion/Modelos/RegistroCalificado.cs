namespace ApiInnovacion.Modelos;

/// <summary>
/// Entidad de dominio de la tabla registro_calificado (v2 — bloque Academico).
/// El documento legal que autoriza el funcionamiento de cada programa.
///
/// Igual que en la v1, no incluye Activo: el borrado lógico es un detalle del
/// motor y no forma parte de lo que la API expone.
/// </summary>
public class RegistroCalificado
{
    /// <summary>codigo (llave primaria).</summary>
    public int Codigo { get; set; }

    /// <summary>cant_creditos.</summary>
    public string CantCreditos { get; set; } = string.Empty;

    /// <summary>hora_acom.</summary>
    public string HoraAcom { get; set; } = string.Empty;

    /// <summary>hora_ind.</summary>
    public string HoraInd { get; set; } = string.Empty;

    /// <summary>metodologia.</summary>
    public string Metodologia { get; set; } = string.Empty;

    /// <summary>fecha_inicio.</summary>
    public DateTime FechaInicio { get; set; }

    /// <summary>fecha_fin.</summary>
    public DateTime FechaFin { get; set; }

    /// <summary>duracion_anios.</summary>
    public string DuracionAnios { get; set; } = string.Empty;

    /// <summary>duracion_semestres.</summary>
    public string DuracionSemestres { get; set; } = string.Empty;

    /// <summary>tipo_titulacion.</summary>
    public string TipoTitulacion { get; set; } = string.Empty;

    /// <summary>programa — referencia a programa.</summary>
    public int Programa { get; set; }
}

/// <summary>
/// Los cambios de un PATCH sobre registro_calificado: todos opcionales, y null significa
/// «no lo toque». La llave no aparece: identifica la fila y no se cambia.
///
/// Vive en Modelos y no en Peticiones porque cruza las capas: el controlador la
/// arma, el servicio la valida y el repositorio la escribe. Es dato del dominio,
/// no el cuerpo HTTP.
/// </summary>
public class RegistroCalificadoCambios
{
    public string? CantCreditos { get; set; }
    public string? HoraAcom { get; set; }
    public string? HoraInd { get; set; }
    public string? Metodologia { get; set; }
    public DateTime? FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    public string? DuracionAnios { get; set; }
    public string? DuracionSemestres { get; set; }
    public string? TipoTitulacion { get; set; }
    public int? Programa { get; set; }

    /// <summary>¿Llegó al menos un campo? Un PATCH vacío es 400, no 404.</summary>
    public bool HayAlgo() =>
        CantCreditos != null ||
        HoraAcom != null ||
        HoraInd != null ||
        Metodologia != null ||
        FechaInicio != null ||
        FechaFin != null ||
        DuracionAnios != null ||
        DuracionSemestres != null ||
        TipoTitulacion != null ||
        Programa != null;
}
