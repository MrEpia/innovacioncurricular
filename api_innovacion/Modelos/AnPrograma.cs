namespace ApiInnovacion.Modelos;

/// <summary>
/// Entidad de dominio de la tabla an_programa (v2 — bloque Puentes).
/// Qué aspectos normativos regulan cada programa.
///
/// Igual que en la v1, no incluye Activo: el borrado lógico es un detalle del
/// motor y no forma parte de lo que la API expone.
/// </summary>
public class AnPrograma
{
    /// <summary>aspecto_normativo (llave primaria) — referencia a aspecto_normativo.</summary>
    public int AspectoNormativo { get; set; }

    /// <summary>programa (llave primaria) — referencia a programa.</summary>
    public int Programa { get; set; }
}
