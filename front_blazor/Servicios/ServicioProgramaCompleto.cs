namespace FrontInnovacion.Servicios;

/// <summary>programa con su detalle: el programa de siempre más tres listas.</summary>
public class ProgramaCompleto : Programa
{
    public List<AreaDelPrograma> AreasConocimiento { get; set; } = new();

    public List<PracticaDelPrograma> PracticasEstrategias { get; set; } = new();

    public List<CaracteristicaDelPrograma> CaracteristicasInnovacion { get; set; } = new();
}

public class AreaDelPrograma
{
    public string AreaConocimiento { get; set; } = string.Empty;

    public string? Nombre { get; set; }
}

public class PracticaDelPrograma
{
    public int? PracticaEstrategia { get; set; }

    public string? Nombre { get; set; }
}

public class CaracteristicaDelPrograma
{
    public int? CarInnovacion { get; set; }

    public string? Nombre { get; set; }
}

/// <summary>La capa de datos del front para el maestro-detalle de programa.</summary>
public class ServicioProgramaCompleto
{
    private readonly ClienteApi _api;

    public ServicioProgramaCompleto(ClienteApi api)
    {
        _api = api;
    }

    // MD4 — El programa con su detalle.
    public Task<Resultado<ProgramaCompleto>> Obtener(int id) =>
        _api.Obtener<ProgramaCompleto>($"/api/programa/{id}/completo");

    // MD3 — Guardar el programa con TODO su detalle: todo o nada.
    public Task<Resultado<bool>> Crear(ProgramaCompleto entidad) =>
        _api.Enviar(HttpMethod.Post, "/api/programa/completo", new
        {
            id = entidad.Id,
            nombre = entidad.Nombre,
            tipo = entidad.Tipo,
            nivel = entidad.Nivel,
            fechaCreacion = entidad.FechaCreacion,
            fechaCierre = string.IsNullOrWhiteSpace(entidad.FechaCierre) ? null : entidad.FechaCierre,
            numeroCohortes = entidad.NumeroCohortes,
            cantGraduados = entidad.CantGraduados,
            fechaActualizacion = entidad.FechaActualizacion,
            ciudad = entidad.Ciudad,
            facultad = entidad.Facultad,
            areasConocimiento = entidad.AreasConocimiento.Select(a => a.AreaConocimiento),
            practicasEstrategias = entidad.PracticasEstrategias.Select(p => p.PracticaEstrategia),
            caracteristicasInnovacion = entidad.CaracteristicasInnovacion.Select(c => c.CarInnovacion)
        });
}
