using ApiInnovacion.Repositorios;
using ApiInnovacion.Servicios;

namespace ApiInnovacion.Ensamblador;

/// <summary>
/// El ensamblador del bloque «Puentes» de la v2: programa_ac, programa_pe, programa_ci, an_programa, enfoque_rc.
///
/// Responsable: David Epia. Este archivo lo llena SOLO su responsable, en su rama:
/// así los tres bloques de la v2 se integran sin tocarse (enmienda 1.1.0 del
/// Artículo 3).
/// </summary>
public static class EnsambladorPuentes
{
    public static IServiceCollection AgregarBloquePuentes(this IServiceCollection servicios)
    {
        servicios.AddScoped<IRepositorioProgramaAc, RepositorioProgramaAcSqlServer>();
        servicios.AddScoped<IServicioProgramaAc, ServicioProgramaAc>();

        servicios.AddScoped<IRepositorioProgramaPe, RepositorioProgramaPeSqlServer>();
        servicios.AddScoped<IServicioProgramaPe, ServicioProgramaPe>();

        servicios.AddScoped<IRepositorioProgramaCi, RepositorioProgramaCiSqlServer>();
        servicios.AddScoped<IServicioProgramaCi, ServicioProgramaCi>();

        servicios.AddScoped<IRepositorioAnPrograma, RepositorioAnProgramaSqlServer>();
        servicios.AddScoped<IServicioAnPrograma, ServicioAnPrograma>();

        servicios.AddScoped<IRepositorioEnfoqueRc, RepositorioEnfoqueRcSqlServer>();
        servicios.AddScoped<IServicioEnfoqueRc, ServicioEnfoqueRc>();

        return servicios;
    }
}
