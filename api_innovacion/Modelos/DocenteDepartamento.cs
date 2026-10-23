namespace ApiInnovacion.Modelos;

/// <summary>
/// Entidad de dominio de la tabla docente_departamento (v2 — bloque Actividades).
/// La vinculación de cada docente a un programa (departamento): dedicación, modalidad y fechas.
///
/// Igual que en la v1, no incluye Activo: el borrado lógico es un detalle del
/// motor y no forma parte de lo que la API expone.
/// </summary>
public class DocenteDepartamento
{
    /// <summary>docente (llave primaria).</summary>
    public int Docente { get; set; }

    /// <summary>departamento (llave primaria) — referencia a programa.</summary>
    public int Departamento { get; set; }

    /// <summary>dedicacion.</summary>
    public string Dedicacion { get; set; } = string.Empty;

    /// <summary>modalidad.</summary>
    public string Modalidad { get; set; } = string.Empty;

    /// <summary>fecha_ingreso.</summary>
    public DateTime FechaIngreso { get; set; }

    /// <summary>fecha_salida — opcional: admite NULL.</summary>
    public DateTime? FechaSalida { get; set; }
}

/// <summary>
/// Los cambios de un PATCH sobre docente_departamento: todos opcionales, y null significa
/// «no lo toque». La llave no aparece: identifica la fila y no se cambia.
///
/// Vive en Modelos y no en Peticiones porque cruza las capas: el controlador la
/// arma, el servicio la valida y el repositorio la escribe. Es dato del dominio,
/// no el cuerpo HTTP.
/// </summary>
public class DocenteDepartamentoCambios
{
    public string? Dedicacion { get; set; }
    public string? Modalidad { get; set; }
    public DateTime? FechaIngreso { get; set; }
    public DateTime? FechaSalida { get; set; }

    /// <summary>¿Llegó al menos un campo? Un PATCH vacío es 400, no 404.</summary>
    public bool HayAlgo() =>
        Dedicacion != null ||
        Modalidad != null ||
        FechaIngreso != null ||
        FechaSalida != null;
}
