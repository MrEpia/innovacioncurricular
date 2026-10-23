namespace ApiInnovacion.Modelos;

/// <summary>
/// MAESTRO-DETALLE de registro_calificado: el registro (maestro) con sus
/// enfoques (enfoque_rc) y sus actividades académicas (aa_rc) como detalle.
///
/// Hereda de RegistroCalificado para que el JSON sea el mismo registro de
/// siempre más dos listas: quien ya conoce el contrato de registro_calificado
/// no tiene que aprender otra forma.
/// </summary>
public class RegistroCalificadoCompleto : RegistroCalificado
{
    public List<EnfoqueDelRegistro> Enfoques { get; set; } = new();

    public List<ActividadDelRegistro> Actividades { get; set; } = new();
}

/// <summary>Una fila de enfoque_rc vista desde su registro calificado.</summary>
public class EnfoqueDelRegistro
{
    public int Enfoque { get; set; }

    /// <summary>Solo de lectura: el nombre del enfoque, para mostrarlo.</summary>
    public string? Nombre { get; set; }
}

/// <summary>Una fila de aa_rc vista desde su registro calificado.</summary>
public class ActividadDelRegistro
{
    public int ActivAcademicasIdcurso { get; set; }

    public string Componente { get; set; } = string.Empty;

    public string Semestre { get; set; } = string.Empty;

    /// <summary>Solo de lectura: el nombre de la actividad, para mostrarlo.</summary>
    public string? Nombre { get; set; }
}
