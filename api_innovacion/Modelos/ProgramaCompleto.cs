namespace ApiInnovacion.Modelos;

/// <summary>
/// MAESTRO-DETALLE de programa: el programa (maestro) con sus áreas de
/// conocimiento (programa_ac), sus prácticas y estrategias (programa_pe) y sus
/// características de innovación (programa_ci) como detalle.
///
/// Hereda de Programa: el JSON es el programa de siempre más tres listas.
/// </summary>
public class ProgramaCompleto : Programa
{
    public List<AreaDelPrograma> AreasConocimiento { get; set; } = new();

    public List<PracticaDelPrograma> PracticasEstrategias { get; set; } = new();

    public List<CaracteristicaDelPrograma> CaracteristicasInnovacion { get; set; } = new();
}

/// <summary>Una fila de programa_ac vista desde su programa.</summary>
public class AreaDelPrograma
{
    public string AreaConocimiento { get; set; } = string.Empty;

    /// <summary>Solo de lectura: el nombre del área, para mostrarlo.</summary>
    public string? Nombre { get; set; }
}

/// <summary>Una fila de programa_pe vista desde su programa.</summary>
public class PracticaDelPrograma
{
    public int PracticaEstrategia { get; set; }

    /// <summary>Solo de lectura.</summary>
    public string? Nombre { get; set; }
}

/// <summary>Una fila de programa_ci vista desde su programa.</summary>
public class CaracteristicaDelPrograma
{
    public int CarInnovacion { get; set; }

    /// <summary>Solo de lectura.</summary>
    public string? Nombre { get; set; }
}
