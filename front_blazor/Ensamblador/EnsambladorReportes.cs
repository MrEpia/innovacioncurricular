using FrontInnovacion.Servicios;

namespace FrontInnovacion.Ensamblador;

/// <summary>v4 — Los servicios del front de reportes y dashboard. Su propio archivo.</summary>
public static class EnsambladorReportes
{
    public static IServiceCollection AgregarReportes(this IServiceCollection servicios)
    {
        servicios.AddScoped<ServicioReportes>();
        servicios.AddScoped<ServicioDashboard>();

        return servicios;
    }
}
