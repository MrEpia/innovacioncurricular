namespace ApiInnovacion.Peticiones;

/// <summary>
/// El cuerpo de POST /api/programa/completo: los campos de ProgramaCrear
/// (heredados, con sus mismas validaciones) más el detalle, como listas de
/// llaves de los catálogos.
/// </summary>
public class ProgramaCompletoCrear : ProgramaCrear
{
    /// <summary>Ids de area_conocimiento (texto de hasta 6). Filas de programa_ac.</summary>
    public List<string> AreasConocimiento { get; set; } = new();

    /// <summary>Ids de practica_estrategia. Filas de programa_pe.</summary>
    public List<int> PracticasEstrategias { get; set; } = new();

    /// <summary>Ids de car_innovacion. Filas de programa_ci.</summary>
    public List<int> CaracteristicasInnovacion { get; set; } = new();
}
