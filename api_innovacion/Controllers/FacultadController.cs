using ApiInnovacion.Excepciones;
using ApiInnovacion.Modelos;
using ApiInnovacion.Peticiones;
using ApiInnovacion.Servicios;
using Microsoft.AspNetCore.Mvc;

namespace ApiInnovacion.Controllers;

/// <summary>
/// La capa 1: HTTP para facultad (v2 — bloque Academico). No contiene
/// lógica de negocio ni SQL: traduce las excepciones del negocio a códigos de
/// estado y las peticiones a lo que la capa 2 entiende.
///
/// La ruta se escribe COMPLETA, con el nombre de la tabla (Artículo 10.1).
/// </summary>
[ApiController]
[Route("api/facultad")]
public class FacultadController : ControllerBase
{
    private readonly IServicioFacultad _servicio;

    public FacultadController(IServicioFacultad servicio)
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

            return Ok(new { tabla = "facultad", limite, total = lista.Count, datos = lista });
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
    public async Task<IActionResult> Crear([FromBody] FacultadCrear peticion)
    {
        try
        {
            var entidad = new Facultad
            {
                Id = peticion.Id!.Value,
                Nombre = peticion.Nombre!,
                Tipo = peticion.Tipo!,
                FechaFun = peticion.FechaFun!.Value,
                Universidad = peticion.Universidad!.Value
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
    public async Task<IActionResult> Reemplazar(int id, [FromBody] FacultadReemplazo peticion)
    {
        try
        {
            var entidad = new Facultad
            {
                Nombre = peticion.Nombre!,
                Tipo = peticion.Tipo!,
                FechaFun = peticion.FechaFun!.Value,
                Universidad = peticion.Universidad!.Value
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
    public async Task<IActionResult> ActualizarParcial(int id, [FromBody] FacultadActualizar peticion)
    {
        try
        {
            var cambios = new FacultadCambios
            {
                Nombre = peticion.Nombre,
                Tipo = peticion.Tipo,
                FechaFun = peticion.FechaFun,
                Universidad = peticion.Universidad
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
        catch (DependenciasActivasExcepcion ex)
        {
            return Conflict(new { estado = 409, mensaje = "Tiene registros que dependen de él.", detalle = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { estado = 500, mensaje = "Error interno del servidor.", detalle = ex.Message });
        }
    }
}
