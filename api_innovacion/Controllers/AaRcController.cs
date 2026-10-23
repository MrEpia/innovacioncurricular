using ApiInnovacion.Excepciones;
using ApiInnovacion.Modelos;
using ApiInnovacion.Peticiones;
using ApiInnovacion.Servicios;
using Microsoft.AspNetCore.Mvc;

namespace ApiInnovacion.Controllers;

/// <summary>
/// La capa 1: HTTP para aa_rc (v2 — bloque Actividades). No contiene
/// lógica de negocio ni SQL: traduce las excepciones del negocio a códigos de
/// estado y las peticiones a lo que la capa 2 entiende.
///
/// La ruta se escribe COMPLETA, con el nombre de la tabla (Artículo 10.1).
/// </summary>
[ApiController]
[Route("api/aa_rc")]
public class AaRcController : ControllerBase
{
    private readonly IServicioAaRc _servicio;

    public AaRcController(IServicioAaRc servicio)
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

            return Ok(new { tabla = "aa_rc", limite, total = lista.Count, datos = lista });
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
    [HttpGet("{activAcademicasIdcurso:int}/{registroCalificadoCodigo:int}")]
    public async Task<IActionResult> ObtenerPorLlave(int activAcademicasIdcurso, int registroCalificadoCodigo)
    {
        try
        {
            return Ok(await _servicio.ObtenerPorLlave(activAcademicasIdcurso, registroCalificadoCodigo));
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
    public async Task<IActionResult> Crear([FromBody] AaRcCrear peticion)
    {
        try
        {
            var entidad = new AaRc
            {
                ActivAcademicasIdcurso = peticion.ActivAcademicasIdcurso!.Value,
                RegistroCalificadoCodigo = peticion.RegistroCalificadoCodigo!.Value,
                Componente = peticion.Componente!,
                Semestre = peticion.Semestre!
            };

            await _servicio.Crear(entidad);
            return StatusCode(201, new { estado = 201, mensaje = "Registro creado exitosamente.", llave = new { activAcademicasIdcurso = entidad.ActivAcademicasIdcurso, registroCalificadoCodigo = entidad.RegistroCalificadoCodigo } });
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
    [HttpPut("{activAcademicasIdcurso:int}/{registroCalificadoCodigo:int}")]
    public async Task<IActionResult> Reemplazar(int activAcademicasIdcurso, int registroCalificadoCodigo, [FromBody] AaRcReemplazo peticion)
    {
        try
        {
            var entidad = new AaRc
            {
                Componente = peticion.Componente!,
                Semestre = peticion.Semestre!
            };

            var filas = await _servicio.Reemplazar(activAcademicasIdcurso, registroCalificadoCodigo, entidad);
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
    [HttpPatch("{activAcademicasIdcurso:int}/{registroCalificadoCodigo:int}")]
    public async Task<IActionResult> ActualizarParcial(int activAcademicasIdcurso, int registroCalificadoCodigo, [FromBody] AaRcActualizar peticion)
    {
        try
        {
            var cambios = new AaRcCambios
            {
                Componente = peticion.Componente,
                Semestre = peticion.Semestre
            };

            var filas = await _servicio.ActualizarParcial(activAcademicasIdcurso, registroCalificadoCodigo, cambios);
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
    [HttpDelete("{activAcademicasIdcurso:int}/{registroCalificadoCodigo:int}")]
    public async Task<IActionResult> Eliminar(int activAcademicasIdcurso, int registroCalificadoCodigo)
    {
        try
        {
            var filas = await _servicio.Eliminar(activAcademicasIdcurso, registroCalificadoCodigo);
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
