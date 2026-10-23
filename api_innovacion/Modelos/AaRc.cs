namespace ApiInnovacion.Modelos;

/// <summary>
/// Entidad de dominio de la tabla aa_rc (v2 — bloque Actividades).
/// Qué actividades académicas incluye cada registro calificado, con su componente y semestre.
///
/// Igual que en la v1, no incluye Activo: el borrado lógico es un detalle del
/// motor y no forma parte de lo que la API expone.
/// </summary>
public class AaRc
{
    /// <summary>activ_academicas_idcurso (llave primaria) — referencia a activ_academica.</summary>
    public int ActivAcademicasIdcurso { get; set; }

    /// <summary>registro_calificado_codigo (llave primaria) — referencia a registro_calificado.</summary>
    public int RegistroCalificadoCodigo { get; set; }

    /// <summary>componente.</summary>
    public string Componente { get; set; } = string.Empty;

    /// <summary>semestre.</summary>
    public string Semestre { get; set; } = string.Empty;
}

/// <summary>
/// Los cambios de un PATCH sobre aa_rc: todos opcionales, y null significa
/// «no lo toque». La llave no aparece: identifica la fila y no se cambia.
///
/// Vive en Modelos y no en Peticiones porque cruza las capas: el controlador la
/// arma, el servicio la valida y el repositorio la escribe. Es dato del dominio,
/// no el cuerpo HTTP.
/// </summary>
public class AaRcCambios
{
    public string? Componente { get; set; }
    public string? Semestre { get; set; }

    /// <summary>¿Llegó al menos un campo? Un PATCH vacío es 400, no 404.</summary>
    public bool HayAlgo() =>
        Componente != null ||
        Semestre != null;
}
