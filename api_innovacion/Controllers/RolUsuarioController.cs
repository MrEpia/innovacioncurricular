using ApiInnovacion.Excepciones;
using ApiInnovacion.Peticiones;
using ApiInnovacion.Seguridad;
using ApiInnovacion.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiInnovacion.Controllers;

/// <summary>
/// /api/rol_usuario — asignar y quitar roles. SOLO administradores. Es una tabla
/// puente sin columnas propias: no tiene PUT ni PATCH (igual que las puentes de
/// la v2).
/// </summary>
[ApiController]
[Authorize(Roles = Roles.Administrador)]
[Route("api/rol_usuario")]
public class RolUsuarioController : ControllerBase
{
    private readonly IServicioRolUsuario _servicio;

    public RolUsuarioController(IServicioRolUsuario servicio)
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
            return Ok(new { tabla = "rol_usuario", limite, total = lista.Count, datos = lista });
        }
        catch (ArgumentException ex) { return BadRequest(new { estado = 400, mensaje = "Parámetros inválidos.", detalle = ex.Message }); }
        catch (Exception ex) { return Error500(ex); }
    }

    [HttpGet("{usuarioId:int}/{rolId:int}")]
    public async Task<IActionResult> ObtenerPorLlave(int usuarioId, int rolId)
    {
        try { return Ok(await _servicio.ObtenerPorLlave(usuarioId, rolId)); }
        catch (NoEncontradoExcepcion ex) { return NotFound(new { estado = 404, mensaje = "Registro no encontrado.", detalle = ex.Message }); }
        catch (Exception ex) { return Error500(ex); }
    }

    [HttpPost]
    public async Task<IActionResult> Asignar([FromBody] RolUsuarioCrear peticion)
    {
        try
        {
            await _servicio.Asignar(peticion.UsuarioId!.Value, peticion.RolId!.Value);
            return StatusCode(201, new
            {
                estado = 201,
                mensaje = "Rol asignado. Tendrá efecto en el próximo ingreso del usuario.",
                llave = new { usuarioId = peticion.UsuarioId, rolId = peticion.RolId }
            });
        }
        catch (LlaveDuplicadaExcepcion ex) { return Conflict(new { estado = 409, mensaje = "La llave ya existe.", detalle = ex.Message }); }
        catch (ArgumentException ex) { return BadRequest(new { estado = 400, mensaje = "Parámetros inválidos.", detalle = ex.Message }); }
        catch (Exception ex) { return Error500(ex); }
    }

    [HttpDelete("{usuarioId:int}/{rolId:int}")]
    public async Task<IActionResult> Quitar(int usuarioId, int rolId)
    {
        try
        {
            var filas = await _servicio.Quitar(usuarioId, rolId);
            return Ok(new { estado = 200, mensaje = "Rol quitado.", filasAfectadas = filas });
        }
        catch (NoEncontradoExcepcion ex) { return NotFound(new { estado = 404, mensaje = "Registro no encontrado.", detalle = ex.Message }); }
        catch (OperacionNoPermitidaExcepcion ex) { return Conflict(new { estado = 409, mensaje = "Operación no permitida.", detalle = ex.Message }); }
        catch (Exception ex) { return Error500(ex); }
    }

    private ObjectResult Error500(Exception ex) =>
        StatusCode(500, new { estado = 500, mensaje = "Error interno del servidor.", detalle = ex.Message });
}
