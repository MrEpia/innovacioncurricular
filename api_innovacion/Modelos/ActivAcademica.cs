namespace ApiInnovacion.Modelos;

/// <summary>
/// Entidad de dominio de la tabla activ_academica (v2 — bloque Actividades).
/// Los cursos y asignaturas: créditos, horas, idioma y si son espejo con otra institución.
///
/// Igual que en la v1, no incluye Activo: el borrado lógico es un detalle del
/// motor y no forma parte de lo que la API expone.
/// </summary>
public class ActivAcademica
{
    /// <summary>id (llave primaria).</summary>
    public int Id { get; set; }

    /// <summary>nombre.</summary>
    public string Nombre { get; set; } = string.Empty;

    /// <summary>num_creditos.</summary>
    public int NumCreditos { get; set; }

    /// <summary>tipo.</summary>
    public string Tipo { get; set; } = string.Empty;

    /// <summary>area_formacion.</summary>
    public string AreaFormacion { get; set; } = string.Empty;

    /// <summary>h_acom.</summary>
    public int HAcom { get; set; }

    /// <summary>h_indep.</summary>
    public int HIndep { get; set; }

    /// <summary>idioma.</summary>
    public string Idioma { get; set; } = string.Empty;

    /// <summary>espejo.</summary>
    public byte Espejo { get; set; }

    /// <summary>entidad_espejo.</summary>
    public string EntidadEspejo { get; set; } = string.Empty;

    /// <summary>pais_espejo.</summary>
    public string PaisEspejo { get; set; } = string.Empty;

    /// <summary>disenio — referencia a programa — opcional: admite NULL.</summary>
    public int? Disenio { get; set; }
}

/// <summary>
/// Los cambios de un PATCH sobre activ_academica: todos opcionales, y null significa
/// «no lo toque». La llave no aparece: identifica la fila y no se cambia.
///
/// Vive en Modelos y no en Peticiones porque cruza las capas: el controlador la
/// arma, el servicio la valida y el repositorio la escribe. Es dato del dominio,
/// no el cuerpo HTTP.
/// </summary>
public class ActivAcademicaCambios
{
    public string? Nombre { get; set; }
    public int? NumCreditos { get; set; }
    public string? Tipo { get; set; }
    public string? AreaFormacion { get; set; }
    public int? HAcom { get; set; }
    public int? HIndep { get; set; }
    public string? Idioma { get; set; }
    public byte? Espejo { get; set; }
    public string? EntidadEspejo { get; set; }
    public string? PaisEspejo { get; set; }
    public int? Disenio { get; set; }

    /// <summary>¿Llegó al menos un campo? Un PATCH vacío es 400, no 404.</summary>
    public bool HayAlgo() =>
        Nombre != null ||
        NumCreditos != null ||
        Tipo != null ||
        AreaFormacion != null ||
        HAcom != null ||
        HIndep != null ||
        Idioma != null ||
        Espejo != null ||
        EntidadEspejo != null ||
        PaisEspejo != null ||
        Disenio != null;
}
