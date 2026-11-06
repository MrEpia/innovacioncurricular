namespace FrontInnovacion.Servicios;

/// <summary>Las cifras de las tarjetas del dashboard.</summary>
public class ResumenDashboard
{
    public int Facultades { get; set; }
    public int Programas { get; set; }
    public int Acreditaciones { get; set; }
    public int RegistrosCalificados { get; set; }
    public int ActividadesAcademicas { get; set; }
    public int Docentes { get; set; }
    public int Alianzas { get; set; }
    public int Pasantias { get; set; }
    public int Premios { get; set; }
    public int Graduados { get; set; }
}

/// <summary>Una barra de un gráfico.</summary>
public class DatoGrafico
{
    public string Etiqueta { get; set; } = string.Empty;
    public int Valor { get; set; }
}

/// <summary>
/// v4 — La capa de datos del front para el dashboard: una ruta por gráfico.
/// Las series vienen en el sobre {grafico, total, datos}, el mismo que lee
/// ClienteApi.Listar.
/// </summary>
public class ServicioDashboard
{
    private readonly ClienteApi _api;

    public ServicioDashboard(ClienteApi api)
    {
        _api = api;
    }

    public Task<Resultado<ResumenDashboard>> Resumen() => _api.Obtener<ResumenDashboard>("/api/dashboard/resumen");
    public Task<Resultado<List<DatoGrafico>>> ProgramasPorNivel() => _api.Listar<DatoGrafico>("/api/dashboard/programas-por-nivel");
    public Task<Resultado<List<DatoGrafico>>> ProgramasPorSede() => _api.Listar<DatoGrafico>("/api/dashboard/programas-por-sede");
    public Task<Resultado<List<DatoGrafico>>> AcreditacionesPorCalificacion() => _api.Listar<DatoGrafico>("/api/dashboard/acreditaciones-por-calificacion");
    public Task<Resultado<List<DatoGrafico>>> RegistrosPorMetodologia() => _api.Listar<DatoGrafico>("/api/dashboard/registros-por-metodologia");
    public Task<Resultado<List<DatoGrafico>>> DocentesPorDedicacion() => _api.Listar<DatoGrafico>("/api/dashboard/docentes-por-dedicacion");
    public Task<Resultado<List<DatoGrafico>>> GraduadosPorFacultad() => _api.Listar<DatoGrafico>("/api/dashboard/graduados-por-facultad?maximo=6");
}
