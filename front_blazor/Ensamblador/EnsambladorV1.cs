using FrontInnovacion.Servicios;

namespace FrontInnovacion.Ensamblador;

/// <summary>
/// El registro de los servicios del front para la v1, y el transporte común.
///
/// Igual que en la API (enmienda 1.1.0 del Artículo 3), cada bloque de la v2
/// registra sus servicios en su propio archivo de Ensamblador/, para que los
/// tres estudiantes no editen Program.cs a la vez.
/// </summary>
public static class EnsambladorV1
{
    public static IServiceCollection AgregarServiciosV1(this IServiceCollection servicios, string urlApi)
    {
        // ============================================================
        // DE DÓNDE SALEN LOS DATOS: de la API, por HTTP, y de ningún otro
        // sitio. Un solo HttpClient configurado —el de ClienteApi— y todos
        // los servicios de recurso lo usan por dentro.
        // ============================================================
        servicios.AddHttpClient<ClienteApi>(cliente =>
        {
            cliente.BaseAddress = new Uri(urlApi);
            cliente.Timeout = TimeSpan.FromSeconds(10);
        });

        // UN SERVICIO POR RECURSO (Artículo 10.1): una línea por tabla, no una
        // que sirva para cualquiera.
        servicios.AddScoped<ServicioAliado>();
        servicios.AddScoped<ServicioAreaConocimiento>();
        servicios.AddScoped<ServicioUniversidad>();
        servicios.AddScoped<ServicioAspectoNormativo>();
        servicios.AddScoped<ServicioPracticaEstrategia>();
        servicios.AddScoped<ServicioEnfoque>();
        servicios.AddScoped<ServicioCarInnovacion>();

        return servicios;
    }
}
