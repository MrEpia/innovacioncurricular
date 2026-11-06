using ApiInnovacion.Modelos;
using ApiInnovacion.Repositorios;
using ApiInnovacion.Servicios;

namespace ApiInnovacion.Pruebas;

/// <summary>
/// v4 — Prueba de capas de los reportes y del dashboard, sin base de datos.
/// Las consultas SQL se prueban contra el motor en humo_reportes.py; aquí se
/// prueba lo que es de la capa 2: el catálogo (10 reportes, cada uno con 4
/// tablas o más y una ruta única) y la regla de «Otras» del dashboard.
/// </summary>
public static class PruebasReportes
{
    public static async Task Ejecutar()
    {
        Console.WriteLine();
        Console.WriteLine("========== v4 · Reportes y dashboard ==========");

        IServicioReportes reportes = new ServicioReportes(new RepositorioReportesFalso());
        var catalogo = reportes.Catalogo();

        Verificador.Revisar(catalogo.Count == 10,
            "reportes: el catálogo tiene exactamente 10 reportes.",
            $"reportes: el catálogo debía tener 10 reportes y tiene {catalogo.Count}.");

        Verificador.Revisar(catalogo.All(r => r.Tablas.Count >= 4),
            "reportes: TODOS unen 4 tablas o más (requisito de la entrega 4).",
            "reportes: hay un reporte con menos de 4 tablas: " +
            string.Join(", ", catalogo.Where(r => r.Tablas.Count < 4).Select(r => r.Ruta)));

        Verificador.Revisar(catalogo.Select(r => r.Ruta).Distinct().Count() == 10,
            "reportes: cada uno tiene su propia ruta.",
            "reportes: hay rutas repetidas en el catálogo.");

        Verificador.Revisar(reportes.Describir("cursos-espejo")?.Titulo == "Cursos espejo internacionales"
                            && reportes.Describir("no-existe") == null,
            "reportes: Describir encuentra un reporte por su ruta y devuelve null si no existe.",
            "reportes: Describir no se comporta como se espera.");

        var filas = await reportes.ProgramasAcreditados();
        Verificador.Revisar(filas.Count == 1 && filas[0].Programa == "Ingeniería de Sistemas",
            "reportes: el servicio entrega las filas que da el repositorio.",
            "reportes: las filas no llegaron tal como las dio el repositorio.");

        Console.WriteLine();
        var tablero = new ServicioDashboard(new RepositorioDashboardFalso());

        var graduados = await tablero.GraduadosPorFacultad(2);
        Verificador.Revisar(graduados.Count == 3 && graduados[2].Etiqueta == "Otras" && graduados[2].Valor == 30,
            "dashboard: con máximo 2, las demás facultades se suman en «Otras» (10 + 20 = 30).",
            $"dashboard: «Otras» debía valer 30 y la serie tener 3 barras; llegó {string.Join(", ", graduados.Select(g => $"{g.Etiqueta}={g.Valor}"))}.");

        Verificador.Revisar(graduados[0].Valor >= graduados[1].Valor,
            "dashboard: las barras van de mayor a menor.",
            "dashboard: las barras no están ordenadas de mayor a menor.");

        var todas = await tablero.GraduadosPorFacultad(10);
        Verificador.Revisar(todas.Count == 4 && todas.All(g => g.Etiqueta != "Otras"),
            "dashboard: si caben todas, no se inventa una barra «Otras».",
            "dashboard: apareció «Otras» sin que sobraran facultades.");

        await Verificador.EsperarExcepcion<ArgumentException>(
            () => tablero.GraduadosPorFacultad(0),
            "dashboard: maximo = 0 es 400.");
    }
}

public class RepositorioReportesFalso : IRepositorioReportes
{
    private static Task<IEnumerable<T>> Vacio<T>() => Task.FromResult(Enumerable.Empty<T>());

    public Task<IEnumerable<ProgramaAcreditado>> ProgramasAcreditados() =>
        Task.FromResult<IEnumerable<ProgramaAcreditado>>(new[] { new ProgramaAcreditado { Programa = "Ingeniería de Sistemas" } });

    public Task<IEnumerable<RegistroConEnfoques>> RegistrosConEnfoques() => Vacio<RegistroConEnfoques>();
    public Task<IEnumerable<ActividadDelPlan>> PlanDeEstudios() => Vacio<ActividadDelPlan>();
    public Task<IEnumerable<AreasDeFacultad>> AreasPorFacultad() => Vacio<AreasDeFacultad>();
    public Task<IEnumerable<InnovacionDePrograma>> InnovacionPorPrograma() => Vacio<InnovacionDePrograma>();
    public Task<IEnumerable<AlianzaVigente>> AlianzasVigentes() => Vacio<AlianzaVigente>();
    public Task<IEnumerable<NormaDePrograma>> NormatividadPorPrograma() => Vacio<NormaDePrograma>();
    public Task<IEnumerable<InternacionalizacionDePrograma>> Internacionalizacion() => Vacio<InternacionalizacionDePrograma>();
    public Task<IEnumerable<DocentesDeSede>> DocentesPorSede() => Vacio<DocentesDeSede>();
    public Task<IEnumerable<CursoEspejo>> CursosEspejo() => Vacio<CursoEspejo>();
}

public class RepositorioDashboardFalso : IRepositorioDashboard
{
    private static Task<IEnumerable<DatoGrafico>> Serie(params (string, int)[] datos) =>
        Task.FromResult(datos.Select(d => new DatoGrafico { Etiqueta = d.Item1, Valor = d.Item2 }));

    public Task<ResumenDashboard> Resumen() => Task.FromResult(new ResumenDashboard());
    public Task<IEnumerable<DatoGrafico>> ProgramasPorNivel() => Serie();
    public Task<IEnumerable<DatoGrafico>> ProgramasPorSede() => Serie();
    public Task<IEnumerable<DatoGrafico>> AcreditacionesPorCalificacion() => Serie();
    public Task<IEnumerable<DatoGrafico>> RegistrosPorMetodologia() => Serie();
    public Task<IEnumerable<DatoGrafico>> DocentesPorDedicacion() => Serie();

    // Desordenadas a propósito: el servicio debe ordenarlas.
    public Task<IEnumerable<DatoGrafico>> GraduadosPorFacultad() =>
        Serie(("Educación", 10), ("Ingeniería", 400), ("Salud", 20), ("Empresariales", 900));
}
