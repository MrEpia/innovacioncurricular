using System.Globalization;
using System.Text.Json;

namespace FrontInnovacion.Servicios;

/// <summary>La ficha de un reporte, tal como la da GET /api/reportes.</summary>
public class FichaReporte
{
    public string Ruta { get; set; } = string.Empty;
    public string Titulo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public List<string> Tablas { get; set; } = new();
}

/// <summary>Un reporte listo para pintar: encabezados y filas ya como texto.</summary>
public class TablaReporte
{
    public List<string> Columnas { get; set; } = new();
    public List<List<string>> Filas { get; set; } = new();
}

/// <summary>
/// v4 — La capa de datos del front para los 10 reportes. Un método POR
/// REPORTE, con su ruta escrita (Artículo 10.1). Cada reporte tiene columnas
/// distintas, así que las filas llegan como JSON y se convierten a texto con
/// los encabezados en español; la pantalla solo pinta una tabla.
/// </summary>
public class ServicioReportes
{
    private readonly ClienteApi _api;

    public ServicioReportes(ClienteApi api)
    {
        _api = api;
    }

    // El catálogo no es un listado con sobre {datos} paginado: es {total, datos}.
    public Task<Resultado<List<FichaReporte>>> Catalogo() => _api.Listar<FichaReporte>("/api/reportes");

    public Task<Resultado<TablaReporte>> ProgramasAcreditados() => Traer("/api/reportes/programas-acreditados");
    public Task<Resultado<TablaReporte>> RegistrosConEnfoques() => Traer("/api/reportes/registros-con-enfoques");
    public Task<Resultado<TablaReporte>> PlanDeEstudios() => Traer("/api/reportes/plan-de-estudios");
    public Task<Resultado<TablaReporte>> AreasPorFacultad() => Traer("/api/reportes/areas-por-facultad");
    public Task<Resultado<TablaReporte>> InnovacionPorPrograma() => Traer("/api/reportes/innovacion-por-programa");
    public Task<Resultado<TablaReporte>> AlianzasVigentes() => Traer("/api/reportes/alianzas-vigentes");
    public Task<Resultado<TablaReporte>> NormatividadPorPrograma() => Traer("/api/reportes/normatividad-por-programa");
    public Task<Resultado<TablaReporte>> Internacionalizacion() => Traer("/api/reportes/internacionalizacion");
    public Task<Resultado<TablaReporte>> DocentesPorSede() => Traer("/api/reportes/docentes-por-sede");
    public Task<Resultado<TablaReporte>> CursosEspejo() => Traer("/api/reportes/cursos-espejo");

    /// <summary>
    /// Qué método corresponde a cada ficha del catálogo. Una tabla escrita, no
    /// una ruta armada con texto: un reporte que la API no tenga, no se pide.
    /// </summary>
    public Func<Task<Resultado<TablaReporte>>>? MetodoDe(string ruta) => ruta switch
    {
        "programas-acreditados" => ProgramasAcreditados,
        "registros-con-enfoques" => RegistrosConEnfoques,
        "plan-de-estudios" => PlanDeEstudios,
        "areas-por-facultad" => AreasPorFacultad,
        "innovacion-por-programa" => InnovacionPorPrograma,
        "alianzas-vigentes" => AlianzasVigentes,
        "normatividad-por-programa" => NormatividadPorPrograma,
        "internacionalizacion" => Internacionalizacion,
        "docentes-por-sede" => DocentesPorSede,
        "cursos-espejo" => CursosEspejo,
        _ => null
    };

    private static readonly Dictionary<string, string> Encabezados = new()
    {
        ["universidad"] = "Universidad", ["ciudad"] = "Ciudad", ["facultad"] = "Facultad",
        ["programa"] = "Programa", ["nivel"] = "Nivel", ["resolucion"] = "Resolución",
        ["tipoAcreditacion"] = "Tipo de acreditación", ["calificacion"] = "Calificación",
        ["desde"] = "Desde", ["hasta"] = "Hasta", ["registro"] = "Registro",
        ["metodologia"] = "Metodología", ["vigenteHasta"] = "Vigente hasta",
        ["cantidadEnfoques"] = "N.º de enfoques", ["enfoques"] = "Enfoques",
        ["actividad"] = "Actividad", ["creditos"] = "Créditos", ["componente"] = "Componente",
        ["semestre"] = "Semestre", ["idioma"] = "Idioma", ["granArea"] = "Gran área",
        ["programas"] = "Programas", ["areas"] = "Áreas", ["caracteristicas"] = "Características de innovación",
        ["practicas"] = "Prácticas y estrategias", ["aliado"] = "Aliado", ["ciudadAliado"] = "Ciudad del aliado",
        ["sede"] = "Sede", ["docente"] = "Docente", ["norma"] = "Norma", ["tipo"] = "Tipo",
        ["fuente"] = "Fuente", ["pasantias"] = "Pasantías", ["paisesPasantia"] = "Países de pasantía",
        ["premios"] = "Premios", ["ultimoPremio"] = "Último premio", ["dedicacion"] = "Dedicación",
        ["docentes"] = "Docentes", ["entidadEspejo"] = "Entidad espejo", ["paisEspejo"] = "País espejo",
    };

    private async Task<Resultado<TablaReporte>> Traer(string ruta)
    {
        var r = await _api.Listar<Dictionary<string, JsonElement>>(ruta);
        if (!r.Ok)
        {
            return Resultado<TablaReporte>.Mal(r.Errores);
        }

        var tabla = new TablaReporte();
        var filas = r.Datos!;
        if (filas.Count == 0)
        {
            return Resultado<TablaReporte>.Bien(tabla);   // 204: el reporte no tiene filas
        }

        var llaves = filas[0].Keys.ToList();
        tabla.Columnas = llaves.Select(k => Encabezados.GetValueOrDefault(k, k)).ToList();
        tabla.Filas = filas.Select(f => llaves.Select(k => Texto(f.GetValueOrDefault(k))).ToList()).ToList();
        return Resultado<TablaReporte>.Bien(tabla);
    }

    private static string Texto(JsonElement valor) => valor.ValueKind switch
    {
        JsonValueKind.Null or JsonValueKind.Undefined => "—",
        JsonValueKind.String when DateTime.TryParse(valor.GetString(), CultureInfo.InvariantCulture,
                                                    DateTimeStyles.None, out var fecha)
                                  && valor.GetString()!.Contains('T') => fecha.ToString("yyyy-MM-dd"),
        JsonValueKind.String => string.IsNullOrEmpty(valor.GetString()) ? "—" : valor.GetString()!,
        _ => valor.ToString()
    };
}
