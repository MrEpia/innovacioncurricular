using FrontInnovacion.Servicios;

namespace FrontInnovacion.Ensamblador;

/// <summary>
/// Los servicios del front del maestro-detalle: registro_calificado y programa
/// con sus hijos. Su propio archivo, como cada bloque.
/// </summary>
public static class EnsambladorMaestroDetalle
{
    public static IServiceCollection AgregarMaestroDetalle(this IServiceCollection servicios)
    {
        servicios.AddScoped<ServicioRegistroCalificadoCompleto>();
        servicios.AddScoped<ServicioProgramaCompleto>();

        return servicios;
    }
}
