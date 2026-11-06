using ApiInnovacion.Modelos;

namespace ApiInnovacion.Repositorios;

/// <summary>
/// v4 — Las 10 consultas multitabla. Un método por consulta, con su nombre:
/// nada de un «Ejecutar(nombreDeReporte)» genérico (Artículo 10.1).
/// Todas leen solo registros activos.
/// </summary>
public interface IRepositorioReportes
{
    Task<IEnumerable<ProgramaAcreditado>> ProgramasAcreditados();
    Task<IEnumerable<RegistroConEnfoques>> RegistrosConEnfoques();
    Task<IEnumerable<ActividadDelPlan>> PlanDeEstudios();
    Task<IEnumerable<AreasDeFacultad>> AreasPorFacultad();
    Task<IEnumerable<InnovacionDePrograma>> InnovacionPorPrograma();
    Task<IEnumerable<AlianzaVigente>> AlianzasVigentes();
    Task<IEnumerable<NormaDePrograma>> NormatividadPorPrograma();
    Task<IEnumerable<InternacionalizacionDePrograma>> Internacionalizacion();
    Task<IEnumerable<DocentesDeSede>> DocentesPorSede();
    Task<IEnumerable<CursoEspejo>> CursosEspejo();
}
