namespace ApiInnovacion.Modelos;

/// <summary>
/// Entidad de dominio de la tabla alianza (v2 — bloque Actividades).
/// Las alianzas formales entre un aliado externo y un programa académico.
///
/// Igual que en la v1, no incluye Activo: el borrado lógico es un detalle del
/// motor y no forma parte de lo que la API expone.
/// </summary>
public class Alianza
{
    /// <summary>aliado (llave primaria) — referencia a aliado.</summary>
    public int Aliado { get; set; }

    /// <summary>departamento (llave primaria) — referencia a programa.</summary>
    public int Departamento { get; set; }

    /// <summary>fecha_inicio.</summary>
    public DateTime FechaInicio { get; set; }

    /// <summary>fecha_fin — opcional: admite NULL.</summary>
    public DateTime? FechaFin { get; set; }

    /// <summary>docente — opcional: admite NULL.</summary>
    public int? Docente { get; set; }
}

/// <summary>
/// Los cambios de un PATCH sobre alianza: todos opcionales, y null significa
/// «no lo toque». La llave no aparece: identifica la fila y no se cambia.
///
/// Vive en Modelos y no en Peticiones porque cruza las capas: el controlador la
/// arma, el servicio la valida y el repositorio la escribe. Es dato del dominio,
/// no el cuerpo HTTP.
/// </summary>
public class AlianzaCambios
{
    public DateTime? FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    public int? Docente { get; set; }

    /// <summary>¿Llegó al menos un campo? Un PATCH vacío es 400, no 404.</summary>
    public bool HayAlgo() =>
        FechaInicio != null ||
        FechaFin != null ||
        Docente != null;
}
