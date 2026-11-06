using ApiInnovacion.Modelos;

namespace ApiInnovacion.Servicios;

/// <summary>v4 — Qué es cada reporte (catálogo) y sus filas.</summary>
public interface IServicioReportes
{
    /// <summary>Los 10 reportes: ruta, título, descripción y tablas que une.</summary>
    IReadOnlyList<DescripcionReporte> Catalogo();

    /// <summary>La descripción de un reporte por su ruta, o null.</summary>
    DescripcionReporte? Describir(string ruta);

    Task<List<ProgramaAcreditado>> ProgramasAcreditados();
    Task<List<RegistroConEnfoques>> RegistrosConEnfoques();
    Task<List<ActividadDelPlan>> PlanDeEstudios();
    Task<List<AreasDeFacultad>> AreasPorFacultad();
    Task<List<InnovacionDePrograma>> InnovacionPorPrograma();
    Task<List<AlianzaVigente>> AlianzasVigentes();
    Task<List<NormaDePrograma>> NormatividadPorPrograma();
    Task<List<InternacionalizacionDePrograma>> Internacionalizacion();
    Task<List<DocentesDeSede>> DocentesPorSede();
    Task<List<CursoEspejo>> CursosEspejo();
}

/// <summary>La ficha de un reporte, para el catálogo y para el sobre de respuesta.</summary>
public record DescripcionReporte(string Ruta, string Titulo, string Descripcion, IReadOnlyList<string> Tablas);
