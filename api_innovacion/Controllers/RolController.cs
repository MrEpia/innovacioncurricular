using ApiInnovacion.Excepciones;
using ApiInnovacion.Modelos;
using ApiInnovacion.Peticiones;
using ApiInnovacion.Seguridad;
using ApiInnovacion.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiInnovacion.Controllers;

/// <summary>/api/rol — SOLO administradores, igual que /api/usuario.</summary>
[ApiController]
[Authorize(Roles = Roles.Administrador)]
[Route("api/rol")]
public class RolController : ControllerBase
{
    private readonly IServicioRol _servicio;

    public RolController(IServicioRol servicio)
    {
        _servicio = servicio;
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerTodos([FromQuery] int limite = 1000)
    {
        try
        {
            var lista = (await _servicio.ObtenerTodos(limite)).ToList();
            if (lista.Count == 0) return NoContent();
            return Ok(new { tabla = "rol", limite, total = lista.Count, datos = lista });
        }
        catch (ArgumentException ex) { return BadRequest(new { estado = 400, mensaje = "Parámetros inválidos.", detalle = ex.Message }); }
        catch (Exception ex) { return Error500(ex); }
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObtenerPorId(int id)
    {
        try { return Ok(await _servicio.ObtenerPorId(id)); }
        catch (NoEncontradoExcepcion ex) { return NotFound(new { estado = 404, mensaje = "Registro no encontrado.", detalle = ex.Message }); }
        catch (Exception ex) { return Error500(ex); }
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] RolCrear peticion)
    {
        try
        {
            var id = await _servicio.Crear(new Rol { Nombre = peticion.Nombre!.Trim(), Descripcion = peticion.Descripcion });
            return StatusCode(201, new { estado = 201, mensaje = "Registro creado exitosamente.", llave = new { id } });
        }
        catch (LlaveDuplicadaExcepcion ex) { return Conflict(new { estado = 409, mensaje = "La llave ya existe.", detalle = ex.Message }); }
        catch (Exception ex) { return Error500(ex); }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Reemplazar(int id, [FromBody] RolReemplazo peticion)
    {
        try
        {
            var filas = await _servicio.Reemplazar(id, new Rol { Nombre = peticion.Nombre!.Trim(), Descripcion = peticion.Descripcion });
            return Ok(new { estado = 200, mensaje = "Registro reemplazado.", filasAfectadas = filas });
        }
        catch (NoEncontradoExcepcion ex) { return NotFound(new { estado = 404, mensaje = "Registro no encontrado.", detalle = ex.Message }); }
        catch (LlaveDuplicadaExcepcion ex) { return Conflict(new { estado = 409, mensaje = "La llave ya existe.", detalle = ex.Message }); }
        catch (OperacionNoPermitidaExcepcion ex) { return Conflict(new { estado = 409, mensaje = "Operación no permitida.", detalle = ex.Message }); }
        catch (Exception ex) { return Error500(ex); }
    }

    [HttpPatch("{id:int}")]
    public async Task<IActionResult> ActualizarParcial(int id, [FromBody] RolActualizar peticion)
    {
        try
        {
            var filas = await _servicio.ActualizarParcial(id, new RolCambios { Nombre = peticion.Nombre?.Trim(), Descripcion = peticion.Descripcion });
            return Ok(new { estado = 200, mensaje = "Registro actualizado.", filasAfectadas = filas });
        }
        catch (NoEncontradoExcepcion ex) { return NotFound(new { estado = 404, mensaje = "Registro no encontrado.", detalle = ex.Message }); }
        catch (LlaveDuplicadaExcepcion ex) { return Conflict(new { estado = 409, mensaje = "La llave ya existe.", detalle = ex.Message }); }
        catch (OperacionNoPermitidaExcepcion ex) { return Conflict(new { estado = 409, mensaje = "Operación no permitida.", detalle = ex.Message }); }
        catch (ArgumentException ex) { return BadRequest(new { estado = 400, mensaje = "Parámetros inválidos.", detalle = ex.Message }); }
        catch (Exception ex) { return Error500(ex); }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        try
        {
            var filas = await _servicio.Eliminar(id);
            return Ok(new { estado = 200, mensaje = "Rol retirado.", filasAfectadas = filas });
        }
        catch (NoEncontradoExcepcion ex) { return NotFound(new { estado = 404, mensaje = "Registro no encontrado.", detalle = ex.Message }); }
        catch (OperacionNoPermitidaExcepcion ex) { return Conflict(new { estado = 409, mensaje = "Operación no permitida.", detalle = ex.Message }); }
        catch (DependenciasActivasExcepcion ex) { return Conflict(new { estado = 409, mensaje = "Tiene registros que dependen de él.", detalle = ex.Message }); }
        catch (Exception ex) { return Error500(ex); }
    }

    private ObjectResult Error500(Exception ex) =>
        StatusCode(500, new { estado = 500, mensaje = "Error interno del servidor.", detalle = ex.Message });
}
