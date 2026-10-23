namespace ApiInnovacion.Modelos;

/// <summary>
/// Entidad de dominio de la tabla programa_ac (v2 — bloque Puentes).
/// Qué áreas de conocimiento cubre cada programa.
///
/// Igual que en la v1, no incluye Activo: el borrado lógico es un detalle del
/// motor y no forma parte de lo que la API expone.
/// </summary>
public class ProgramaAc
{
    /// <summary>programa (llave primaria) — referencia a programa.</summary>
    public int Programa { get; set; }

    /// <summary>area_conocimiento (llave primaria) — referencia a area_conocimiento.</summary>
    public string AreaConocimiento { get; set; } = string.Empty;
}
