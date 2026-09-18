using ApiInnovacion.Excepciones;
using ApiInnovacion.Modelos;
using ApiInnovacion.Peticiones;
using ApiInnovacion.Servicios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace ApiInnovacion.Controllers;

/// <summary>
/// La capa 1: HTTP para area_conocimiento. No contiene lógica de negocio ni SQL. Traduce
/// las excepciones del negocio a códigos de estado, y las peticiones a lo que
/// la capa 2 entiende.
///
/// La ruta se escribe COMPLETA y no con [controller], para que coincida
/// exactamente con el nombre de la tabla que pide el contrato.
/// </summary>
[ApiController]
[Route("api/area_conocimiento")]
public class AreaConocimientoController : ControllerBase
{
    private readonly IServicioAreaConocimiento _servicio;

    public AreaConocimientoController(IServicioAreaConocimiento servicio)
    {
        _servicio = servicio;
    }

    /// <summary>RF1 — Listar registros activos.</summary>
    [HttpGet]
    public async Task<IActionResult> ObtenerTodos([FromQuery] int limite = 1000)
    {
        try
        {
            var datos = await _servicio.ObtenerTodos(limite);
            var lista = datos.ToList();

            // Vacío NO es error: 204 sin cuerpo.
            if (lista.Count == 0)
            {
                return NoContent();
            }

            return Ok(new
            {
                tabla = "area_conocimiento",
                limite = limite,
                total = lista.Count,
                datos = lista
            });
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

    /// <summary>RF2 — Obtener un registro por su llave.</summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> ObtenerPorId(string id)
    {
        try
        {
            var entidad = await _servicio.ObtenerPorId(id);
            return Ok(entidad);
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

    /// <summary>RF3 — Crear un registro.</summary>
    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] AreaConocimientoCrear peticion)
    {
        try
        {
            var entidad = new AreaConocimiento
            {
                Id = peticion.Id,
                GranArea = peticion.GranArea,
                Area = peticion.Area,
                Disciplina = peticion.Disciplina
            };

            await _servicio.Crear(entidad);
            return Ok(new { estado = 200, mensaje = "Registro creado exitosamente." });
        }
        catch (SqlException ex)
        {
            // Llave duplicada: la defiende la base, no la API.
            return StatusCode(500, new { estado = 500, mensaje = "Error al insertar en la base de datos.", detalle = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { estado = 500, mensaje = "Error interno del servidor.", detalle = ex.Message });
        }
    }

    /// <summary>RF4 — Reemplazar completamente un registro.</summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Reemplazar(string id, [FromBody] AreaConocimientoReemplazo peticion)
    {
        try
        {
            var entidad = new AreaConocimiento
            {
                Id = id,
                GranArea = peticion.GranArea,
                Area = peticion.Area,
                Disciplina = peticion.Disciplina
            };

            var filas = await _servicio.Reemplazar(id, entidad);
            return Ok(new { estado = 200, mensaje = "Registro reemplazado.", filasAfectadas = filas });
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

    /// <summary>RF5 — Actualizar parcialmente un registro.</summary>
    [HttpPatch("{id}")]
    public async Task<IActionResult> ActualizarParcial(string id, [FromBody] AreaConocimientoActualizar peticion)
    {
        try
        {
            var filas = await _servicio.ActualizarParcial(id,
                peticion.GranArea, peticion.Area, peticion.Disciplina);

            return Ok(new { estado = 200, mensaje = "Registro actualizado.", filasAfectadas = filas });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { estado = 400, mensaje = "Parámetros inválidos.", detalle = ex.Message });
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

    /// <summary>RF6 — Borrado lógico.</summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Eliminar(string id)
    {
        try
        {
            var filas = await _servicio.Eliminar(id);
            return Ok(new { estado = 200, mensaje = "Registro eliminado.", filasAfectadas = filas });
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
