using ApiInnovacion.Servicios;
using Microsoft.AspNetCore.Mvc;

namespace ApiInnovacion.Controllers;

/// <summary>
/// v4 — Las 10 CONSULTAS MULTITABLA, cada una con su ruta escrita completa:
/// /api/reportes/programas-acreditados, /api/reportes/plan-de-estudios… No
/// existe un /api/reportes/{nombre} genérico (Artículo 10.1): Swagger documenta
/// cada reporte con sus columnas.
///
/// Solo lectura (GET): el filtro global los deja ver a los tres roles.
/// Sobre: { reporte, titulo, tablas, total, datos }. Sin filas → 204.
/// </summary>
[ApiController]
[Route("api/reportes")]
public class ReportesController : ControllerBase
{
    private readonly IServicioReportes _servicio;

    public ReportesController(IServicioReportes servicio)
    {
        _servicio = servicio;
    }

    /// <summary>El catálogo: los 10 reportes con su ruta, título, descripción y tablas.</summary>
    [HttpGet]
    public IActionResult Catalogo() =>
        Ok(new { total = _servicio.Catalogo().Count, datos = _servicio.Catalogo() });

    /// <summary>R1 — Programas acreditados (acreditacion, programa, facultad, universidad).</summary>
    [HttpGet("programas-acreditados")]
    public Task<IActionResult> ProgramasAcreditados() => Responder("programas-acreditados", _servicio.ProgramasAcreditados);

    /// <summary>R2 — Registros calificados con sus enfoques (5 tablas).</summary>
    [HttpGet("registros-con-enfoques")]
    public Task<IActionResult> RegistrosConEnfoques() => Responder("registros-con-enfoques", _servicio.RegistrosConEnfoques);

    /// <summary>R3 — Plan de estudios por registro calificado (4 tablas).</summary>
    [HttpGet("plan-de-estudios")]
    public Task<IActionResult> PlanDeEstudios() => Responder("plan-de-estudios", _servicio.PlanDeEstudios);

    /// <summary>R4 — Áreas de conocimiento por facultad (4 tablas).</summary>
    [HttpGet("areas-por-facultad")]
    public Task<IActionResult> AreasPorFacultad() => Responder("areas-por-facultad", _servicio.AreasPorFacultad);

    /// <summary>R5 — Innovación por programa (6 tablas).</summary>
    [HttpGet("innovacion-por-programa")]
    public Task<IActionResult> InnovacionPorPrograma() => Responder("innovacion-por-programa", _servicio.InnovacionPorPrograma);

    /// <summary>R6 — Alianzas vigentes (5 tablas).</summary>
    [HttpGet("alianzas-vigentes")]
    public Task<IActionResult> AlianzasVigentes() => Responder("alianzas-vigentes", _servicio.AlianzasVigentes);

    /// <summary>R7 — Normatividad por programa (4 tablas).</summary>
    [HttpGet("normatividad-por-programa")]
    public Task<IActionResult> NormatividadPorPrograma() => Responder("normatividad-por-programa", _servicio.NormatividadPorPrograma);

    /// <summary>R8 — Internacionalización y reconocimientos (4 tablas).</summary>
    [HttpGet("internacionalizacion")]
    public Task<IActionResult> Internacionalizacion() => Responder("internacionalizacion", _servicio.Internacionalizacion);

    /// <summary>R9 — Docentes por sede y dedicación (4 tablas).</summary>
    [HttpGet("docentes-por-sede")]
    public Task<IActionResult> DocentesPorSede() => Responder("docentes-por-sede", _servicio.DocentesPorSede);

    /// <summary>R10 — Cursos espejo internacionales (4 tablas).</summary>
    [HttpGet("cursos-espejo")]
    public Task<IActionResult> CursosEspejo() => Responder("cursos-espejo", _servicio.CursosEspejo);

    /// <summary>
    /// El sobre y los errores, escritos una vez. No es una ruta genérica: cada
    /// acción de arriba sigue teniendo su ruta y su tipo de fila.
    /// </summary>
    private async Task<IActionResult> Responder<T>(string ruta, Func<Task<List<T>>> consulta)
    {
        try
        {
            var datos = await consulta();
            if (datos.Count == 0)
            {
                return NoContent();
            }

            var ficha = _servicio.Describir(ruta)!;
            return Ok(new { reporte = ficha.Ruta, titulo = ficha.Titulo, tablas = ficha.Tablas, total = datos.Count, datos });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { estado = 500, mensaje = "Error interno del servidor.", detalle = ex.Message });
        }
    }
}
