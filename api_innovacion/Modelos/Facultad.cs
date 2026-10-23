namespace ApiInnovacion.Modelos;

/// <summary>
/// Entidad de dominio de la tabla facultad (v2 — bloque Academico).
/// Las facultades, departamentos e institutos de cada sede de la universidad.
///
/// Igual que en la v1, no incluye Activo: el borrado lógico es un detalle del
/// motor y no forma parte de lo que la API expone.
/// </summary>
public class Facultad
{
    /// <summary>id (llave primaria).</summary>
    public int Id { get; set; }

    /// <summary>nombre.</summary>
    public string Nombre { get; set; } = string.Empty;

    /// <summary>tipo.</summary>
    public string Tipo { get; set; } = string.Empty;

    /// <summary>fecha_fun.</summary>
    public DateTime FechaFun { get; set; }

    /// <summary>universidad — referencia a universidad.</summary>
    public int Universidad { get; set; }
}

/// <summary>
/// Los cambios de un PATCH sobre facultad: todos opcionales, y null significa
/// «no lo toque». La llave no aparece: identifica la fila y no se cambia.
///
/// Vive en Modelos y no en Peticiones porque cruza las capas: el controlador la
/// arma, el servicio la valida y el repositorio la escribe. Es dato del dominio,
/// no el cuerpo HTTP.
/// </summary>
public class FacultadCambios
{
    public string? Nombre { get; set; }
    public string? Tipo { get; set; }
    public DateTime? FechaFun { get; set; }
    public int? Universidad { get; set; }

    /// <summary>¿Llegó al menos un campo? Un PATCH vacío es 400, no 404.</summary>
    public bool HayAlgo() =>
        Nombre != null ||
        Tipo != null ||
        FechaFun != null ||
        Universidad != null;
}
