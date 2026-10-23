namespace ApiInnovacion.Modelos;

/// <summary>
/// Entidad de dominio de la tabla enfoque_rc (v2 — bloque Puentes).
/// Qué enfoques pedagógicos declara cada registro calificado.
///
/// Igual que en la v1, no incluye Activo: el borrado lógico es un detalle del
/// motor y no forma parte de lo que la API expone.
/// </summary>
public class EnfoqueRc
{
    /// <summary>enfoque (llave primaria) — referencia a enfoque.</summary>
    public int Enfoque { get; set; }

    /// <summary>registro_calificado (llave primaria) — referencia a registro_calificado.</summary>
    public int RegistroCalificado { get; set; }
}
