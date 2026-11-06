using ApiInnovacion.Modelos;
using ApiInnovacion.Servicios;
using Microsoft.AspNetCore.Mvc;

namespace ApiInnovacion.Controllers;

/// <summary>
/// v4 — Los endpoints que alimentan el DASHBOARD con datos agregados: un
/// resumen de cifras y una serie {etiqueta, valor} por gráfico. Una ruta por
/// gráfico (Artículo 10.1). Solo lectura: GET para los tres roles.
/// </summary>
[ApiController]
[Route("api/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly IServicioDashboard _servicio;

    public DashboardController(IServicioDashboard servicio)
    {
        _servicio = servicio;
    }

    /// <summary>Las cifras de las tarjetas: facultades, programas, docentes, graduados…</summary>
    [HttpGet("resumen")]
    public async Task<IActionResult> Resumen()
    {
        try
        {
            return Ok(await _servicio.Resumen());
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { estado = 500, mensaje = "Error interno del servidor.", detalle = ex.Message });
        }
    }

    /// <summary>Programas activos por nivel (profesional, maestría…).</summary>
    [HttpGet("programas-por-nivel")]
    public Task<IActionResult> ProgramasPorNivel() => Serie("programas-por-nivel", _servicio.ProgramasPorNivel);

    /// <summary>Programas activos por ciudad de la sede.</summary>
    [HttpGet("programas-por-sede")]
    public Task<IActionResult> ProgramasPorSede() => Serie("programas-por-sede", _servicio.ProgramasPorSede);

    /// <summary>Acreditaciones activas por calificación.</summary>
    [HttpGet("acreditaciones-por-calificacion")]
    public Task<IActionResult> AcreditacionesPorCalificacion() => Serie("acreditaciones-por-calificacion", _servicio.AcreditacionesPorCalificacion);

    /// <summary>Registros calificados activos por metodología.</summary>
    [HttpGet("registros-por-metodologia")]
    public Task<IActionResult> RegistrosPorMetodologia() => Serie("registros-por-metodologia", _servicio.RegistrosPorMetodologia);

    /// <summary>Vinculaciones docentes activas por dedicación.</summary>
    [HttpGet("docentes-por-dedicacion")]
    public Task<IActionResult> DocentesPorDedicacion() => Serie("docentes-por-dedicacion", _servicio.DocentesPorDedicacion);

    /// <summary>Graduados por facultad: las «maximo» principales y el resto en «Otras».</summary>
    [HttpGet("graduados-por-facultad")]
    public Task<IActionResult> GraduadosPorFacultad([FromQuery] int maximo = 6) =>
        Serie("graduados-por-facultad", () => _servicio.GraduadosPorFacultad(maximo));

    private async Task<IActionResult> Serie(string grafico, Func<Task<List<DatoGrafico>>> consulta)
    {
        try
        {
            var datos = await consulta();
            if (datos.Count == 0)
            {
                return NoContent();
            }

            return Ok(new { grafico, total = datos.Sum(d => d.Valor), datos });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { estado = 400, mensaje = "Parámetros inválidos.", detalle = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { estado = 500, mensaje = "Error interno del servidor.", detalle = ex.Message });
        }
    }
}
