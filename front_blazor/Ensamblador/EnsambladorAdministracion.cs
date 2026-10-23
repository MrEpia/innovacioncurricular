using FrontInnovacion.Servicios;

namespace FrontInnovacion.Ensamblador;

/// <summary>
/// Los servicios del panel de administración (v3) — Sebastián Brunal: usuario,
/// rol y rol_usuario. Usan el ClienteApi de EnsambladorV1, que ya les pone el
/// token de la sesión.
/// </summary>
public static class EnsambladorAdministracion
{
    public static IServiceCollection AgregarAdministracion(this IServiceCollection servicios)
    {
        servicios.AddScoped<ServicioUsuario>();
        servicios.AddScoped<ServicioRol>();
        servicios.AddScoped<ServicioRolUsuario>();
        return servicios;
    }
}
