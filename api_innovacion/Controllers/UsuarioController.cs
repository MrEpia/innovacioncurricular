using ApiInnovacion.Excepciones;
using ApiInnovacion.Modelos;
using ApiInnovacion.Peticiones;
using ApiInnovacion.Seguridad;
using ApiInnovacion.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiInnovacion.Controllers;

/// <summary>
/// /api/usuario — SOLO administradores (PDF, entrega 3). El [Authorize] se
/// suma al filtro global de permisos por verbo: aquí, además de tener token,
/// hay que ser administrador incluso para LEER. Un editor que pida la lista de
/// usuarios recibe 403 aunque su token sea válido.
/// </summary>
[ApiController]
[Authorize(Roles = Roles.Administrador)]
[Route("api/usuario")]
public class UsuarioController : ControllerBase
{
    private readonly IServicioUsuario _servicio;

    public UsuarioController(IServicioUsuario servicio)
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
            return Ok(new { tabla = "usuario", limite, total = lista.Count, datos = lista });
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
    public async Task<IActionResult> Crear([FromBody] UsuarioCrear peticion)
    {
        try
        {
            var id = await _servicio.Crear(new Usuario
            {
                Username = peticion.Username!.Trim(),
                Email = peticion.Email!.Trim(),
                NombreCompleto = peticion.NombreCompleto
            }, peticion.Clave!);

            return StatusCode(201, new { estado = 201, mensaje = "Usuario creado. Asígnele un rol para que pueda ingresar.", llave = new { id } });
        }
        catch (LlaveDuplicadaExcepcion ex) { return Conflict(new { estado = 409, mensaje = "La llave ya existe.", detalle = ex.Message }); }
        catch (Exception ex) { return Error500(ex); }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Reemplazar(int id, [FromBody] UsuarioReemplazo peticion)
    {
        try
        {
            var filas = await _servicio.Reemplazar(id, new Usuario
            {
                Username = peticion.Username!.Trim(),
                Email = peticion.Email!.Trim(),
                NombreCompleto = peticion.NombreCompleto
            });
            return Ok(new { estado = 200, mensaje = "Registro reemplazado.", filasAfectadas = filas });
        }
        catch (NoEncontradoExcepcion ex) { return NotFound(new { estado = 404, mensaje = "Registro no encontrado.", detalle = ex.Message }); }
        catch (LlaveDuplicadaExcepcion ex) { return Conflict(new { estado = 409, mensaje = "La llave ya existe.", detalle = ex.Message }); }
        catch (Exception ex) { return Error500(ex); }
    }

    [HttpPatch("{id:int}")]
    public async Task<IActionResult> ActualizarParcial(int id, [FromBody] UsuarioActualizar peticion)
    {
        try
        {
            var filas = await _servicio.ActualizarParcial(id, new UsuarioCambios
            {
                Username = peticion.Username?.Trim(),
                Email = peticion.Email?.Trim(),
                NombreCompleto = peticion.NombreCompleto,
                Clave = peticion.Clave
            });
            return Ok(new { estado = 200, mensaje = "Registro actualizado.", filasAfectadas = filas });
        }
        catch (NoEncontradoExcepcion ex) { return NotFound(new { estado = 404, mensaje = "Registro no encontrado.", detalle = ex.Message }); }
        catch (LlaveDuplicadaExcepcion ex) { return Conflict(new { estado = 409, mensaje = "La llave ya existe.", detalle = ex.Message }); }
        catch (ArgumentException ex) { return BadRequest(new { estado = 400, mensaje = "Parámetros inválidos.", detalle = ex.Message }); }
        catch (Exception ex) { return Error500(ex); }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        try
        {
            var filas = await _servicio.Eliminar(id);
            return Ok(new { estado = 200, mensaje = "Usuario retirado.", filasAfectadas = filas });
        }
        catch (NoEncontradoExcepcion ex) { return NotFound(new { estado = 404, mensaje = "Registro no encontrado.", detalle = ex.Message }); }
        catch (OperacionNoPermitidaExcepcion ex) { return Conflict(new { estado = 409, mensaje = "Operación no permitida.", detalle = ex.Message }); }
        catch (Exception ex) { return Error500(ex); }
    }

    private ObjectResult Error500(Exception ex) =>
        StatusCode(500, new { estado = 500, mensaje = "Error interno del servidor.", detalle = ex.Message });
}
