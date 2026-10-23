using ApiInnovacion.Excepciones;
using ApiInnovacion.Modelos;
using ApiInnovacion.Peticiones;
using ApiInnovacion.Servicios;
using Microsoft.AspNetCore.Mvc;

namespace ApiInnovacion.Controllers;

/// <summary>
/// La capa 1: HTTP para alianza (v2 — bloque Actividades). No contiene
/// lógica de negocio ni SQL: traduce las excepciones del negocio a códigos de
/// estado y las peticiones a lo que la capa 2 entiende.
///
/// La ruta se escribe COMPLETA, con el nombre de la tabla (Artículo 10.1).
/// </summary>
[ApiController]
[Route("api/alianza")]
public class AlianzaController : ControllerBase
{
    private readonly IServicioAlianza _servicio;

    public AlianzaController(IServicioAlianza servicio)
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

            return Ok(new { tabla = "alianza", limite, total = lista.Count, datos = lista });
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
    [HttpGet("{aliado:int}/{departamento:int}")]
    public async Task<IActionResult> ObtenerPorLlave(int aliado, int departamento)
    {
        try
        {
            return Ok(await _servicio.ObtenerPorLlave(aliado, departamento));
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
    public async Task<IActionResult> Crear([FromBody] AlianzaCrear peticion)
    {
        try
        {
            var entidad = new Alianza
            {
                Aliado = peticion.Aliado!.Value,
                Departamento = peticion.Departamento!.Value,
                FechaInicio = peticion.FechaInicio!.Value,
                FechaFin = peticion.FechaFin,
                Docente = peticion.Docente
            };

            await _servicio.Crear(entidad);
            return StatusCode(201, new { estado = 201, mensaje = "Registro creado exitosamente.", llave = new { aliado = entidad.Aliado, departamento = entidad.Departamento } });
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
    [HttpPut("{aliado:int}/{departamento:int}")]
    public async Task<IActionResult> Reemplazar(int aliado, int departamento, [FromBody] AlianzaReemplazo peticion)
    {
        try
        {
            var entidad = new Alianza
            {
                FechaInicio = peticion.FechaInicio!.Value,
                FechaFin = peticion.FechaFin,
                Docente = peticion.Docente
            };

            var filas = await _servicio.Reemplazar(aliado, departamento, entidad);
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
    [HttpPatch("{aliado:int}/{departamento:int}")]
    public async Task<IActionResult> ActualizarParcial(int aliado, int departamento, [FromBody] AlianzaActualizar peticion)
    {
        try
        {
            var cambios = new AlianzaCambios
            {
                FechaInicio = peticion.FechaInicio,
                FechaFin = peticion.FechaFin,
                Docente = peticion.Docente
            };

            var filas = await _servicio.ActualizarParcial(aliado, departamento, cambios);
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
    [HttpDelete("{aliado:int}/{departamento:int}")]
    public async Task<IActionResult> Eliminar(int aliado, int departamento)
    {
        try
        {
            var filas = await _servicio.Eliminar(aliado, departamento);
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
