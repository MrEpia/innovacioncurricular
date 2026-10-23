namespace ApiInnovacion.Modelos;

/// <summary>
/// Entidad de dominio de la tabla programa_pe (v2 — bloque Puentes).
/// Qué prácticas y estrategias pedagógicas aplica cada programa.
///
/// Igual que en la v1, no incluye Activo: el borrado lógico es un detalle del
/// motor y no forma parte de lo que la API expone.
/// </summary>
public class ProgramaPe
{
    /// <summary>programa (llave primaria) — referencia a programa.</summary>
    public int Programa { get; set; }

    /// <summary>practica_estrategia (llave primaria) — referencia a practica_estrategia.</summary>
    public int PracticaEstrategia { get; set; }
}
