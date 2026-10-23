namespace ApiInnovacion.Modelos;

/// <summary>
/// Entidad de dominio de la tabla acreditacion (v2 — bloque Academico).
/// Las acreditaciones de alta calidad otorgadas a los programas, por número de resolución.
///
/// Igual que en la v1, no incluye Activo: el borrado lógico es un detalle del
/// motor y no forma parte de lo que la API expone.
/// </summary>
public class Acreditacion
{
    /// <summary>resolucion (llave primaria).</summary>
    public int Resolucion { get; set; }

    /// <summary>tipo.</summary>
    public string Tipo { get; set; } = string.Empty;

    /// <summary>calificacion.</summary>
    public string Calificacion { get; set; } = string.Empty;

    /// <summary>fecha_inicio.</summary>
    public string FechaInicio { get; set; } = string.Empty;

    /// <summary>fecha_fin.</summary>
    public string FechaFin { get; set; } = string.Empty;

    /// <summary>programa — referencia a programa.</summary>
    public int Programa { get; set; }
}

/// <summary>
/// Los cambios de un PATCH sobre acreditacion: todos opcionales, y null significa
/// «no lo toque». La llave no aparece: identifica la fila y no se cambia.
///
/// Vive en Modelos y no en Peticiones porque cruza las capas: el controlador la
/// arma, el servicio la valida y el repositorio la escribe. Es dato del dominio,
/// no el cuerpo HTTP.
/// </summary>
public class AcreditacionCambios
{
    public string? Tipo { get; set; }
    public string? Calificacion { get; set; }
    public string? FechaInicio { get; set; }
    public string? FechaFin { get; set; }
    public int? Programa { get; set; }

    /// <summary>¿Llegó al menos un campo? Un PATCH vacío es 400, no 404.</summary>
    public bool HayAlgo() =>
        Tipo != null ||
        Calificacion != null ||
        FechaInicio != null ||
        FechaFin != null ||
        Programa != null;
}
