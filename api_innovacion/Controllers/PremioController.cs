using ApiInnovacion.Excepciones;
using ApiInnovacion.Modelos;
using ApiInnovacion.Peticiones;
using ApiInnovacion.Servicios;
using Microsoft.AspNetCore.Mvc;

namespace ApiInnovacion.Controllers;

/// <summary>
/// La capa 1: HTTP para premio (v2 — bloque Actividades). No contiene
/// lógica de negocio ni SQL: traduce las excepciones del negocio a códigos de
/// estado y las peticiones a lo que la capa 2 entiende.
///
/// La ruta se escribe COMPLETA, con el nombre de la tabla (Artículo 10.1).
/// </summary>
[ApiController]
[Route("api/premio")]
public class PremioController : ControllerBase
{
    private readonly IServicioPremio _servicio;

    public PremioController(IServicioPremio servicio)
    {
        _servicio = servicio;
    }

    /// <summary>RF1 — Listar registros activos.</summary>
    [HttpGet]
    public async Task<IActionResult> ObtenerTodos([FromQuery] int limite = 1000)
    {
        try
        {
            var lista = (await _servicio.ObtenerTodos(limite)).ToList();

            // Vacío NO es error: 204 sin cuerpo.
            if (lista.Count == 0)
            {
                return NoContent();
            }

            return Ok(new { tabla = "premio", limite, total = lista.Count, datos = lista });
        }
        catch (ArgumentException ex)
        {
            // Incluye ReferenciaInvalidaExcepcion: una clave foránea que no apunta
            // a un registro activo es una regla de negocio rota.
            return BadRequest(new { estado = 400, mensaje = "Parámetros inválidos.", detalle = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { estado = 500, mensaje = "Error interno del servidor.", detalle = ex.Message });
        }
    }
    /// <summary>RF2 — Obtener un registro por su llave.</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObtenerPorId(int id)
    {
        try
        {
            return Ok(await _servicio.ObtenerPorId(id));
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
    /// <summary>RF3 — Crear un registro. 201 con la llave creada.</summary>
    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] PremioCrear peticion)
    {
        try
        {
            var entidad = new Premio
            {
                Id = peticion.Id!.Value,
                Nombre = peticion.Nombre!,
                Descripcion = peticion.Descripcion!,
                Fecha = peticion.Fecha!.Value,
                EntidadOtorgante = peticion.EntidadOtorgante!,
                Pais = peticion.Pais!,
                Programa = peticion.Programa!.Value
            };

            await _servicio.Crear(entidad);
            return StatusCode(201, new { estado = 201, mensaje = "Registro creado exitosamente.", llave = new { id = entidad.Id } });
        }
        catch (LlaveDuplicadaExcepcion ex)
        {
            return Conflict(new { estado = 409, mensaje = "La llave ya existe.", detalle = ex.Message });
        }
        catch (ArgumentException ex)
        {
            // Incluye ReferenciaInvalidaExcepcion: una clave foránea que no apunta
            // a un registro activo es una regla de negocio rota.
            return BadRequest(new { estado = 400, mensaje = "Parámetros inválidos.", detalle = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { estado = 500, mensaje = "Error interno del servidor.", detalle = ex.Message });
        }
    }
    /// <summary>RF4 — Reemplazar completamente un registro.</summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Reemplazar(int id, [FromBody] PremioReemplazo peticion)
    {
        try
        {
            var entidad = new Premio
            {
                Nombre = peticion.Nombre!,
                Descripcion = peticion.Descripcion!,
                Fecha = peticion.Fecha!.Value,
                EntidadOtorgante = peticion.EntidadOtorgante!,
                Pais = peticion.Pais!,
                Programa = peticion.Programa!.Value
            };

            var filas = await _servicio.Reemplazar(id, entidad);
            return Ok(new { estado = 200, mensaje = "Registro reemplazado.", filasAfectadas = filas });
        }
        catch (NoEncontradoExcepcion ex)
        {
            return NotFound(new { estado = 404, mensaje = "Registro no encontrado.", detalle = ex.Message });
        }
        catch (ArgumentException ex)
        {
            // Incluye ReferenciaInvalidaExcepcion: una clave foránea que no apunta
            // a un registro activo es una regla de negocio rota.
            return BadRequest(new { estado = 400, mensaje = "Parámetros inválidos.", detalle = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { estado = 500, mensaje = "Error interno del servidor.", detalle = ex.Message });
        }
    }
    /// <summary>RF5 — Actualizar parcialmente un registro.</summary>
    [HttpPatch("{id:int}")]
    public async Task<IActionResult> ActualizarParcial(int id, [FromBody] PremioActualizar peticion)
    {
        try
        {
            var cambios = new PremioCambios
            {
                Nombre = peticion.Nombre,
                Descripcion = peticion.Descripcion,
                Fecha = peticion.Fecha,
                EntidadOtorgante = peticion.EntidadOtorgante,
                Pais = peticion.Pais,
                Programa = peticion.Programa
            };

            var filas = await _servicio.ActualizarParcial(id, cambios);
            return Ok(new { estado = 200, mensaje = "Registro actualizado.", filasAfectadas = filas });
        }
        catch (NoEncontradoExcepcion ex)
        {
            return NotFound(new { estado = 404, mensaje = "Registro no encontrado.", detalle = ex.Message });
        }
        catch (ArgumentException ex)
        {
            // Incluye ReferenciaInvalidaExcepcion: una clave foránea que no apunta
            // a un registro activo es una regla de negocio rota.
            return BadRequest(new { estado = 400, mensaje = "Parámetros inválidos.", detalle = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { estado = 500, mensaje = "Error interno del servidor.", detalle = ex.Message });
        }
    }
    /// <summary>RF6 — Borrado lógico.</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        try
        {
            var filas = await _servicio.Eliminar(id);
            return Ok(new { estado = 200, mensaje = "Registro retirado.", filasAfectadas = filas });
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
