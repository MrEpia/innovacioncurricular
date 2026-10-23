namespace ApiInnovacion.Modelos;

/// <summary>
/// Entidad de dominio de la tabla programa (v2 — bloque Academico).
/// Los programas de pregrado y posgrado que ofrece cada facultad.
///
/// Igual que en la v1, no incluye Activo: el borrado lógico es un detalle del
/// motor y no forma parte de lo que la API expone.
/// </summary>
public class Programa
{
    /// <summary>id (llave primaria).</summary>
    public int Id { get; set; }

    /// <summary>nombre.</summary>
    public string Nombre { get; set; } = string.Empty;

    /// <summary>tipo.</summary>
    public string Tipo { get; set; } = string.Empty;

    /// <summary>nivel.</summary>
    public string Nivel { get; set; } = string.Empty;

    /// <summary>fecha_creacion.</summary>
    public string FechaCreacion { get; set; } = string.Empty;

    /// <summary>fecha_cierre — opcional: admite NULL.</summary>
    public string? FechaCierre { get; set; }

    /// <summary>numero_cohortes.</summary>
    public string NumeroCohortes { get; set; } = string.Empty;

    /// <summary>cant_graduados.</summary>
    public string CantGraduados { get; set; } = string.Empty;

    /// <summary>fecha_actualizacion.</summary>
    public string FechaActualizacion { get; set; } = string.Empty;

    /// <summary>ciudad.</summary>
    public string Ciudad { get; set; } = string.Empty;

    /// <summary>facultad — referencia a facultad.</summary>
    public int Facultad { get; set; }
}

/// <summary>
/// Los cambios de un PATCH sobre programa: todos opcionales, y null significa
/// «no lo toque». La llave no aparece: identifica la fila y no se cambia.
///
/// Vive en Modelos y no en Peticiones porque cruza las capas: el controlador la
/// arma, el servicio la valida y el repositorio la escribe. Es dato del dominio,
/// no el cuerpo HTTP.
/// </summary>
public class ProgramaCambios
{
    public string? Nombre { get; set; }
    public string? Tipo { get; set; }
    public string? Nivel { get; set; }
    public string? FechaCreacion { get; set; }
    public string? FechaCierre { get; set; }
    public string? NumeroCohortes { get; set; }
    public string? CantGraduados { get; set; }
    public string? FechaActualizacion { get; set; }
    public string? Ciudad { get; set; }
    public int? Facultad { get; set; }

    /// <summary>¿Llegó al menos un campo? Un PATCH vacío es 400, no 404.</summary>
    public bool HayAlgo() =>
        Nombre != null ||
        Tipo != null ||
        Nivel != null ||
        FechaCreacion != null ||
        FechaCierre != null ||
        NumeroCohortes != null ||
        CantGraduados != null ||
        FechaActualizacion != null ||
        Ciudad != null ||
        Facultad != null;
}
