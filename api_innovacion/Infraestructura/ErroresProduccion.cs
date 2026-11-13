using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ApiInnovacion.Infraestructura;

/// <summary>Los controladores históricos traducen excepciones; filtrar el detalle 500 antes de publicarlo.</summary>
public sealed class ErroresProduccion(IHostEnvironment entorno, ILogger<ErroresProduccion> logger) : IResultFilter
{
    public void OnResultExecuting(ResultExecutingContext contexto)
    {
        if (!entorno.IsDevelopment() && contexto.Result is ObjectResult { StatusCode: >= 500 } resultado)
        {
            logger.LogError("Respuesta interna HTTP {Estado}; referencia {Referencia}", resultado.StatusCode, contexto.HttpContext.TraceIdentifier);
            resultado.Value = new { estado = resultado.StatusCode, mensaje = "Error interno del servidor.", detalle = "Consulte los registros con la referencia indicada.", referencia = contexto.HttpContext.TraceIdentifier };
        }
    }

    public void OnResultExecuted(ResultExecutedContext contexto) { }
}
