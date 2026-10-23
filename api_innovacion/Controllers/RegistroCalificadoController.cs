using ApiInnovacion.Excepciones;
using ApiInnovacion.Modelos;
using ApiInnovacion.Peticiones;
using ApiInnovacion.Servicios;
using Microsoft.AspNetCore.Mvc;

namespace ApiInnovacion.Controllers;

/// <summary>
/// La capa 1: HTTP para registro_calificado (v2 — bloque Academico). No contiene
/// lógica de negocio ni SQL: traduce las excepciones del negocio a códigos de
/// estado y las peticiones a lo que la capa 2 entiende.
///
/// La ruta se escribe COMPLETA, con el nombre de la tabla (Artículo 10.1).
/// </summary>
[ApiController]
[Route("api/registro_calificado")]
public class RegistroCalificadoController : ControllerBase
{
    private readonly IServicioRegistroCalificado _servicio;

    public RegistroCalificadoController(IServicioRegistroCalificado servicio)
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

            return Ok(new { tabla = "registro_calificado", limite, total = lista.Count, datos = lista });
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
    [HttpGet("{codigo:int}")]
    public async Task<IActionResult> ObtenerPorCodigo(int codigo)
    {
        try
        {
            return Ok(await _servicio.ObtenerPorCodigo(codigo));
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
    public async Task<IActionResult> Crear([FromBody] RegistroCalificadoCrear peticion)
    {
        try
        {
            var entidad = new RegistroCalificado
            {
                Codigo = peticion.Codigo!.Value,
                CantCreditos = peticion.CantCreditos!,
                HoraAcom = peticion.HoraAcom!,
                HoraInd = peticion.HoraInd!,
                Metodologia = peticion.Metodologia!,
                FechaInicio = peticion.FechaInicio!.Value,
                FechaFin = peticion.FechaFin!.Value,
                DuracionAnios = peticion.DuracionAnios!,
                DuracionSemestres = peticion.DuracionSemestres!,
                TipoTitulacion = peticion.TipoTitulacion!,
                Programa = peticion.Programa!.Value
            };

            await _servicio.Crear(entidad);
            return StatusCode(201, new { estado = 201, mensaje = "Registro creado exitosamente.", llave = new { codigo = entidad.Codigo } });
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
    [HttpPut("{codigo:int}")]
    public async Task<IActionResult> Reemplazar(int codigo, [FromBody] RegistroCalificadoReemplazo peticion)
    {
        try
        {
            var entidad = new RegistroCalificado
            {
                CantCreditos = peticion.CantCreditos!,
                HoraAcom = peticion.HoraAcom!,
                HoraInd = peticion.HoraInd!,
                Metodologia = peticion.Metodologia!,
                FechaInicio = peticion.FechaInicio!.Value,
                FechaFin = peticion.FechaFin!.Value,
                DuracionAnios = peticion.DuracionAnios!,
                DuracionSemestres = peticion.DuracionSemestres!,
                TipoTitulacion = peticion.TipoTitulacion!,
                Programa = peticion.Programa!.Value
            };

            var filas = await _servicio.Reemplazar(codigo, entidad);
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
    [HttpPatch("{codigo:int}")]
    public async Task<IActionResult> ActualizarParcial(int codigo, [FromBody] RegistroCalificadoActualizar peticion)
    {
        try
        {
            var cambios = new RegistroCalificadoCambios
            {
                CantCreditos = peticion.CantCreditos,
                HoraAcom = peticion.HoraAcom,
                HoraInd = peticion.HoraInd,
                Metodologia = peticion.Metodologia,
                FechaInicio = peticion.FechaInicio,
                FechaFin = peticion.FechaFin,
                DuracionAnios = peticion.DuracionAnios,
                DuracionSemestres = peticion.DuracionSemestres,
                TipoTitulacion = peticion.TipoTitulacion,
                Programa = peticion.Programa
            };

            var filas = await _servicio.ActualizarParcial(codigo, cambios);
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
    [HttpDelete("{codigo:int}")]
    public async Task<IActionResult> Eliminar(int codigo)
    {
        try
        {
            var filas = await _servicio.Eliminar(codigo);
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
