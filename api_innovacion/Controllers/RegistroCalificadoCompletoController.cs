using ApiInnovacion.Excepciones;
using ApiInnovacion.Modelos;
using ApiInnovacion.Peticiones;
using ApiInnovacion.Servicios;
using Microsoft.AspNetCore.Mvc;

namespace ApiInnovacion.Controllers;

/// <summary>
/// La capa 1 del MAESTRO-DETALLE de registro_calificado: el registro con sus
/// enfoques y sus actividades en una sola petición.
///
/// Vive bajo la ruta de su tabla —/api/registro_calificado/completo—, no en un
/// /api/maestro/{tabla} genérico (Artículo 10.1). Es otro controlador para no
/// mezclar dos recursos en la misma clase. Los permisos los pone el filtro
/// global: GET para todos los roles, POST para administrador y editor.
/// </summary>
[ApiController]
[Route("api/registro_calificado")]
public class RegistroCalificadoCompletoController : ControllerBase
{
    private readonly IServicioRegistroCalificadoCompleto _servicio;

    public RegistroCalificadoCompletoController(IServicioRegistroCalificadoCompleto servicio)
    {
        _servicio = servicio;
    }

    /// <summary>MD1 — Crear el registro con todo su detalle. Todo o nada.</summary>
    [HttpPost("completo")]
    public async Task<IActionResult> Crear([FromBody] RegistroCalificadoCompletoCrear peticion)
    {
        try
        {
            var entidad = new RegistroCalificadoCompleto
            {
                Codigo = peticion.Codigo!.Value,
                CantCreditos = peticion.CantCreditos!,
                HoraAcom = peticion.HoraAcom!,
                HoraInd = peticion.HoraInd!,
                Metodologia = peticion.Metodologia!,
                FechaInicio = peticion.FechaInicio!.Value,
                FechaFin = peticion.FechaFin!.Value,
                DuracionAnios = peticion.DuracionAnios!,
                DuracionSemestres = peticion.DuracionSemestres!,
                TipoTitulacion = peticion.TipoTitulacion!,
                Programa = peticion.Programa!.Value,
                Enfoques = peticion.Enfoques.Select(e => new EnfoqueDelRegistro { Enfoque = e }).ToList(),
                Actividades = peticion.Actividades.Select(a => new ActividadDelRegistro
                {
                    ActivAcademicasIdcurso = a.ActivAcademicasIdcurso!.Value,
                    Componente = a.Componente!,
                    Semestre = a.Semestre!
                }).ToList()
            };

            await _servicio.Crear(entidad);
            return StatusCode(201, new
            {
                estado = 201,
                mensaje = "Registro creado con todo su detalle.",
                llave = new { codigo = entidad.Codigo },
                detalle = new { enfoques = entidad.Enfoques.Count, actividades = entidad.Actividades.Count }
            });
        }
        catch (LlaveDuplicadaExcepcion ex)
        {
            return Conflict(new { estado = 409, mensaje = "La llave ya existe.", detalle = ex.Message });
        }
        catch (ArgumentException ex)
        {
            // Incluye ReferenciaInvalidaExcepcion: un padre o un hijo que no
            // apunta a un registro activo.
            return BadRequest(new { estado = 400, mensaje = "Parámetros inválidos.", detalle = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { estado = 500, mensaje = "Error interno del servidor.", detalle = ex.Message });
        }
    }

    /// <summary>MD2 — El registro con su detalle activo.</summary>
    [HttpGet("{codigo:int}/completo")]
    public async Task<IActionResult> ObtenerCompleto(int codigo)
    {
        try
        {
            return Ok(await _servicio.ObtenerCompleto(codigo));
        }
        catch (NoEncontradoExcepcion ex)
        {
            return NotFound(new { estado = 404, mensaje = "Registro no encontrado.", detalle = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { estado = 500, mensaje = "Error interno del servidor.", detalle = ex.Message });
        }
    }
}
