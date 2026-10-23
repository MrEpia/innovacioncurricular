namespace ApiInnovacion.Modelos;

/// <summary>
/// Entidad de dominio de la tabla premio (v2 — bloque Actividades).
/// Los premios y reconocimientos recibidos por los programas, con la entidad que los otorga.
///
/// Igual que en la v1, no incluye Activo: el borrado lógico es un detalle del
/// motor y no forma parte de lo que la API expone.
/// </summary>
public class Premio
{
    /// <summary>id (llave primaria).</summary>
    public int Id { get; set; }

    /// <summary>nombre.</summary>
    public string Nombre { get; set; } = string.Empty;

    /// <summary>descripcion.</summary>
    public string Descripcion { get; set; } = string.Empty;

    /// <summary>fecha.</summary>
    public DateTime Fecha { get; set; }

    /// <summary>entidad_otorgante.</summary>
    public string EntidadOtorgante { get; set; } = string.Empty;

    /// <summary>pais.</summary>
    public string Pais { get; set; } = string.Empty;

    /// <summary>programa — referencia a programa.</summary>
    public int Programa { get; set; }
}

/// <summary>
/// Los cambios de un PATCH sobre premio: todos opcionales, y null significa
/// «no lo toque». La llave no aparece: identifica la fila y no se cambia.
///
/// Vive en Modelos y no en Peticiones porque cruza las capas: el controlador la
/// arma, el servicio la valida y el repositorio la escribe. Es dato del dominio,
/// no el cuerpo HTTP.
/// </summary>
public class PremioCambios
{
    public string? Nombre { get; set; }
    public string? Descripcion { get; set; }
    public DateTime? Fecha { get; set; }
    public string? EntidadOtorgante { get; set; }
    public string? Pais { get; set; }
    public int? Programa { get; set; }

    /// <summary>¿Llegó al menos un campo? Un PATCH vacío es 400, no 404.</summary>
    public bool HayAlgo() =>
        Nombre != null ||
        Descripcion != null ||
        Fecha != null ||
        EntidadOtorgante != null ||
        Pais != null ||
        Programa != null;
}
