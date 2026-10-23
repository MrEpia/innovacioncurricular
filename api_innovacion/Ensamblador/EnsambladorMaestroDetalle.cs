using ApiInnovacion.Repositorios;
using ApiInnovacion.Servicios;

namespace ApiInnovacion.Ensamblador;

/// <summary>
/// El maestro-detalle (registro_calificado y programa con sus hijos). Su
/// propio archivo, como cada bloque (D-v2-1): no toca los de los compañeros.
/// </summary>
public static class EnsambladorMaestroDetalle
{
    public static IServiceCollection AgregarMaestroDetalle(this IServiceCollection servicios)
    {
        servicios.AddScoped<IRepositorioRegistroCalificadoCompleto, RepositorioRegistroCalificadoCompletoSqlServer>();
        servicios.AddScoped<IServicioRegistroCalificadoCompleto, ServicioRegistroCalificadoCompleto>();

        servicios.AddScoped<IRepositorioProgramaCompleto, RepositorioProgramaCompletoSqlServer>();
        servicios.AddScoped<IServicioProgramaCompleto, ServicioProgramaCompleto>();

        return servicios;
    }
}
