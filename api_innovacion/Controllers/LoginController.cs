using ApiInnovacion.Excepciones;
using ApiInnovacion.Peticiones;
using ApiInnovacion.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiInnovacion.Controllers;

/// <summary>
/// POST /api/login — la ÚNICA ruta de la API que responde sin token (además
/// del diagnóstico y de Swagger). [AllowAnonymous] la saca del filtro global de
/// permisos: sin esto nadie podría conseguir su primer token.
/// </summary>
[ApiController]
[AllowAnonymous]
[Route("api/login")]
public class LoginController : ControllerBase
{
    private readonly IServicioAutenticacion _servicio;

    public LoginController(IServicioAutenticacion servicio)
    {
        _servicio = servicio;
    }

    [HttpPost]
    public async Task<IActionResult> IniciarSesion([FromBody] LoginPeticion peticion)
    {
        try
        {
            var r = await _servicio.IniciarSesion(peticion.Usuario!, peticion.Clave!);
            return Ok(new
            {
                token = r.Token,
                tipo = "Bearer",
                expira = r.ExpiraUtc,
                usuario = new { id = r.UsuarioId, username = r.Username, nombreCompleto = r.NombreCompleto, roles = r.Roles }
            });
        }
        catch (CredencialesInvalidasExcepcion ex)
        {
            return StatusCode(401, new { estado = 401, mensaje = "No autenticado.", detalle = ex.Message });
        }
        catch (OperacionNoPermitidaExcepcion ex)
        {
            return StatusCode(403, new { estado = 403, mensaje = "Sin permiso.", detalle = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { estado = 500, mensaje = "Error interno del servidor.", detalle = ex.Message });
        }
    }
}
