using FrontInnovacion.Servicios;

namespace FrontInnovacion.Ensamblador;

/// <summary>
/// Los servicios del front del bloque «Puentes» de la v2: programa_ac, programa_pe, programa_ci, an_programa, enfoque_rc.
/// Responsable: David Epia. Solo su responsable edita este archivo.
///
/// Todos usan por dentro el ClienteApi que registra EnsambladorV1: aquí solo
/// se declara que cada recurso tiene su servicio (Artículo 10.1).
/// </summary>
public static class EnsambladorPuentes
{
    public static IServiceCollection AgregarServiciosPuentes(this IServiceCollection servicios)
    {
        servicios.AddScoped<ServicioProgramaAc>();
        servicios.AddScoped<ServicioProgramaPe>();
        servicios.AddScoped<ServicioProgramaCi>();
        servicios.AddScoped<ServicioAnPrograma>();
        servicios.AddScoped<ServicioEnfoqueRc>();

        return servicios;
    }
}
