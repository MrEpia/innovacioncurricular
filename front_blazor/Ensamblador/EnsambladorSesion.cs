using FrontInnovacion.Servicios;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace FrontInnovacion.Ensamblador;

/// <summary>
/// La sesión del front (v3) — David Epia.
///
/// AgregarSesion registra la autenticación por cookie con la sesión en el
/// servidor; UsarSesion pone los middlewares y las dos direcciones que no son
/// pantallas: cerrar sesión y sesión vencida.
/// </summary>
public static class EnsambladorSesion
{
    public static IServiceCollection AgregarSesion(this IServiceCollection servicios, string urlApi)
    {
        servicios.AddMemoryCache();
        servicios.AddSingleton<ITicketStore, AlmacenSesionesMemoria>();

        servicios
            .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(cookie =>
            {
                cookie.LoginPath = "/ingresar";
                cookie.AccessDeniedPath = "/sin-permiso";
                cookie.Cookie.Name = ".Innovacion.Sesion";
                cookie.Cookie.HttpOnly = true;             // ningún script de la página la lee
                cookie.Cookie.SameSite = SameSiteMode.Lax;
                // Dura lo que dura el token y ni un minuto más: no se renueva sola.
                cookie.SlidingExpiration = false;
            });

        // El ticket va al almacén del SERVIDOR; la cookie solo lleva su llave.
        servicios.AddOptions<CookieAuthenticationOptions>(CookieAuthenticationDefaults.AuthenticationScheme)
            .Configure<ITicketStore>((opciones, almacen) => opciones.SessionStore = almacen);

        servicios.AddAuthorization();

        // Las pantallas (estáticas e interactivas) reciben el usuario y sus roles.
        servicios.AddCascadingAuthenticationState();

        servicios.AddScoped<ProveedorToken>();
        servicios.AddHttpClient<ServicioSesion>(cliente =>
        {
            cliente.BaseAddress = new Uri(urlApi);
            cliente.Timeout = TimeSpan.FromSeconds(10);
        });

        return servicios;
    }

    public static void UsarSesion(this WebApplication app)
    {
        app.UseAuthentication();
        app.UseAuthorization();
    }

    /// <summary>Las dos direcciones de la sesión que no son pantallas.</summary>
    public static void MapearSesion(this WebApplication app)
    {
        // CERRAR SESIÓN (RF10). POST con token antifalsificación: un enlace en
        // otra página no puede cerrarle la sesión a nadie. Al cerrar, el
        // almacén BORRA el ticket: el JWT deja de existir en el servidor.
        app.MapPost("/sesion/salir", async (HttpContext http, IAntiforgery antifalsificacion) =>
        {
            await antifalsificacion.ValidateRequestAsync(http);
            await http.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Results.Redirect("/ingresar?salio=1");
        }).DisableAntiforgery();   // se valida a mano arriba, con mensaje propio si falla

        // SESIÓN VENCIDA (RF10): ClienteApi trae aquí al usuario cuando la API
        // responde 401. Se cierra la sesión y se pide ingresar de nuevo.
        app.MapGet("/sesion/expirada", async (HttpContext http) =>
        {
            await http.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Results.Redirect("/ingresar?expirada=1");
        });
    }
}
