namespace ApiInnovacion.Modelos;

/// <summary>
/// Entidad de dominio de la tabla programa_ci (v2 — bloque Puentes).
/// Qué características de innovación curricular tiene cada programa.
///
/// Igual que en la v1, no incluye Activo: el borrado lógico es un detalle del
/// motor y no forma parte de lo que la API expone.
/// </summary>
public class ProgramaCi
{
    /// <summary>programa (llave primaria) — referencia a programa.</summary>
    public int Programa { get; set; }

    /// <summary>car_innovacion (llave primaria) — referencia a car_innovacion.</summary>
    public int CarInnovacion { get; set; }
}
