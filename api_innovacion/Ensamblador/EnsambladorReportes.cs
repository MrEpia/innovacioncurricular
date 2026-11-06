using ApiInnovacion.Repositorios;
using ApiInnovacion.Servicios;

namespace ApiInnovacion.Ensamblador;

/// <summary>v4 — Reportes (10 consultas multitabla) y dashboard. Su propio archivo (D-v2-1).</summary>
public static class EnsambladorReportes
{
    public static IServiceCollection AgregarReportes(this IServiceCollection servicios)
    {
        servicios.AddScoped<IRepositorioReportes, RepositorioReportesSqlServer>();
        servicios.AddScoped<IServicioReportes, ServicioReportes>();

        servicios.AddScoped<IRepositorioDashboard, RepositorioDashboardSqlServer>();
        servicios.AddScoped<IServicioDashboard, ServicioDashboard>();

        return servicios;
    }
}
