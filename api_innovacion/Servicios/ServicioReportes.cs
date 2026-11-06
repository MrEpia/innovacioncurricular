using ApiInnovacion.Modelos;
using ApiInnovacion.Repositorios;

namespace ApiInnovacion.Servicios;

/// <summary>
/// v4 — La capa 2 de los reportes. Las consultas ya vienen resueltas del
/// repositorio; lo que esta capa sabe es QUÉ es cada reporte: su título, para
/// qué sirve y qué tablas une. El front arma su menú de reportes con este
/// catálogo, y la prueba de capas comprueba que todos unan 4 tablas o más.
/// </summary>
public class ServicioReportes : IServicioReportes
{
    private readonly IRepositorioReportes _repositorio;

    public ServicioReportes(IRepositorioReportes repositorio)
    {
        _repositorio = repositorio;
    }

    private static readonly IReadOnlyList<DescripcionReporte> _catalogo = new List<DescripcionReporte>
    {
        new("programas-acreditados", "Programas acreditados",
            "Cada acreditación vigente con su programa, su facultad y su sede.",
            new[] { "acreditacion", "programa", "facultad", "universidad" }),
        new("registros-con-enfoques", "Registros calificados y sus enfoques",
            "Los enfoques pedagógicos declarados en cada registro calificado.",
            new[] { "registro_calificado", "programa", "facultad", "enfoque_rc", "enfoque" }),
        new("plan-de-estudios", "Plan de estudios por registro",
            "Las actividades académicas de cada registro, con componente y semestre.",
            new[] { "aa_rc", "registro_calificado", "programa", "activ_academica" }),
        new("areas-por-facultad", "Áreas de conocimiento por facultad",
            "Cuántos programas y áreas tiene cada facultad en cada gran área.",
            new[] { "facultad", "programa", "programa_ac", "area_conocimiento" }),
        new("innovacion-por-programa", "Innovación por programa",
            "Las características de innovación y las prácticas pedagógicas de cada programa.",
            new[] { "programa", "facultad", "programa_ci", "car_innovacion", "programa_pe", "practica_estrategia" }),
        new("alianzas-vigentes", "Alianzas vigentes",
            "Las alianzas activas y no vencidas, con el programa y la sede.",
            new[] { "alianza", "aliado", "programa", "facultad", "universidad" }),
        new("normatividad-por-programa", "Normatividad por programa",
            "Qué normas rigen cada programa.",
            new[] { "an_programa", "aspecto_normativo", "programa", "facultad" }),
        new("internacionalizacion", "Internacionalización y reconocimientos",
            "Pasantías, países y premios de cada programa.",
            new[] { "programa", "facultad", "pasantia", "premio" }),
        new("docentes-por-sede", "Docentes por sede y dedicación",
            "Cuántos docentes vigentes tiene cada facultad, por dedicación.",
            new[] { "docente_departamento", "programa", "facultad", "universidad" }),
        new("cursos-espejo", "Cursos espejo internacionales",
            "Las actividades académicas que se dictan en espejo con una universidad extranjera.",
            new[] { "activ_academica", "programa", "facultad", "universidad" }),
    };

    public IReadOnlyList<DescripcionReporte> Catalogo() => _catalogo;

    public DescripcionReporte? Describir(string ruta) => _catalogo.FirstOrDefault(r => r.Ruta == ruta);

    public async Task<List<ProgramaAcreditado>> ProgramasAcreditados() => (await _repositorio.ProgramasAcreditados()).ToList();
    public async Task<List<RegistroConEnfoques>> RegistrosConEnfoques() => (await _repositorio.RegistrosConEnfoques()).ToList();
    public async Task<List<ActividadDelPlan>> PlanDeEstudios() => (await _repositorio.PlanDeEstudios()).ToList();
    public async Task<List<AreasDeFacultad>> AreasPorFacultad() => (await _repositorio.AreasPorFacultad()).ToList();
    public async Task<List<InnovacionDePrograma>> InnovacionPorPrograma() => (await _repositorio.InnovacionPorPrograma()).ToList();
    public async Task<List<AlianzaVigente>> AlianzasVigentes() => (await _repositorio.AlianzasVigentes()).ToList();
    public async Task<List<NormaDePrograma>> NormatividadPorPrograma() => (await _repositorio.NormatividadPorPrograma()).ToList();
    public async Task<List<InternacionalizacionDePrograma>> Internacionalizacion() => (await _repositorio.Internacionalizacion()).ToList();
    public async Task<List<DocentesDeSede>> DocentesPorSede() => (await _repositorio.DocentesPorSede()).ToList();
    public async Task<List<CursoEspejo>> CursosEspejo() => (await _repositorio.CursosEspejo()).ToList();
}
