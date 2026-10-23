using ApiInnovacion.Excepciones;
using ApiInnovacion.Modelos;
using ApiInnovacion.Peticiones;
using ApiInnovacion.Servicios;
using Microsoft.AspNetCore.Mvc;

namespace ApiInnovacion.Controllers;

/// <summary>
/// La capa 1 del MAESTRO-DETALLE de programa: el programa con sus áreas de
/// conocimiento, prácticas y características de innovación en una sola
/// petición. Ruta bajo su tabla (Artículo 10.1).
/// </summary>
[ApiController]
[Route("api/programa")]
public class ProgramaCompletoController : ControllerBase
{
    private readonly IServicioProgramaCompleto _servicio;

    public ProgramaCompletoController(IServicioProgramaCompleto servicio)
    {
        _servicio = servicio;
    }

    /// <summary>MD3 — Crear el programa con todo su detalle. Todo o nada.</summary>
    [HttpPost("completo")]
    public async Task<IActionResult> Crear([FromBody] ProgramaCompletoCrear peticion)
    {
        try
        {
            var entidad = new ProgramaCompleto
            {
                Id = peticion.Id!.Value,
                Nombre = peticion.Nombre!,
                Tipo = peticion.Tipo!,
                Nivel = peticion.Nivel!,
                FechaCreacion = peticion.FechaCreacion!,
                FechaCierre = peticion.FechaCierre,
                NumeroCohortes = peticion.NumeroCohortes!,
                CantGraduados = peticion.CantGraduados!,
                FechaActualizacion = peticion.FechaActualizacion!,
                Ciudad = peticion.Ciudad!,
                Facultad = peticion.Facultad!.Value,
                AreasConocimiento = peticion.AreasConocimiento.Select(a => new AreaDelPrograma { AreaConocimiento = a }).ToList(),
                PracticasEstrategias = peticion.PracticasEstrategias.Select(p => new PracticaDelPrograma { PracticaEstrategia = p }).ToList(),
                CaracteristicasInnovacion = peticion.CaracteristicasInnovacion.Select(c => new CaracteristicaDelPrograma { CarInnovacion = c }).ToList()
            };

            await _servicio.Crear(entidad);
            return StatusCode(201, new
            {
                estado = 201,
                mensaje = "Programa creado con todo su detalle.",
                llave = new { id = entidad.Id },
                detalle = new
                {
                    areasConocimiento = entidad.AreasConocimiento.Count,
                    practicasEstrategias = entidad.PracticasEstrategias.Count,
                    caracteristicasInnovacion = entidad.CaracteristicasInnovacion.Count
                }
            });
        }
        catch (LlaveDuplicadaExcepcion ex)
        {
            return Conflict(new { estado = 409, mensaje = "La llave ya existe.", detalle = ex.Message });
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

    /// <summary>MD4 — El programa con su detalle activo.</summary>
    [HttpGet("{id:int}/completo")]
    public async Task<IActionResult> ObtenerCompleto(int id)
    {
        try
        {
            return Ok(await _servicio.ObtenerCompleto(id));
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
