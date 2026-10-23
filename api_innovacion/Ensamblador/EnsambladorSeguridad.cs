using System.Security.Claims;
using System.Text;
using ApiInnovacion.Repositorios;
using ApiInnovacion.Seguridad;
using ApiInnovacion.Servicios;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace ApiInnovacion.Ensamblador;

/// <summary>
/// El ensamblador de la v3: autenticación JWT, permisos por rol y el CRUD de
/// usuario, rol y rol_usuario. Responsable: Sebastián Calvo.
///
/// Son dos métodos porque son dos momentos: AgregarSeguridad REGISTRA los
/// servicios (antes de construir la aplicación) y UsarSeguridad pone los
/// middlewares en la tubería (después).
/// </summary>
public static class EnsambladorSeguridad
{
    public static IServiceCollection AgregarSeguridad(this IServiceCollection servicios, IConfiguration configuracion)
    {
        // 1. La configuración del JWT, validada AL ARRANCAR: sin secreto o con
        //    uno corto la API se niega a subir (Artículo 7).
        var opciones = configuracion.GetSection("Jwt").Get<OpcionesJwt>() ?? new OpcionesJwt();
        opciones.Validar();
        servicios.Configure<OpcionesJwt>(configuracion.GetSection("Jwt"));

        // 2. Las piezas de la v3, como siempre: interfaz → clase concreta.
        servicios.AddSingleton<IHasheadorClaves, HasheadorClavesPbkdf2>();
        servicios.AddSingleton<IGeneradorToken, GeneradorTokenJwt>();
        servicios.AddScoped<IRepositorioUsuario, RepositorioUsuarioSqlServer>();
        servicios.AddScoped<IRepositorioRol, RepositorioRolSqlServer>();
        servicios.AddScoped<IRepositorioRolUsuario, RepositorioRolUsuarioSqlServer>();
        servicios.AddScoped<IServicioAutenticacion, ServicioAutenticacion>();
        servicios.AddScoped<IServicioUsuario, ServicioUsuario>();
        servicios.AddScoped<IServicioRol, ServicioRol>();
        servicios.AddScoped<IServicioRolUsuario, ServicioRolUsuario>();

        // 3. AUTENTICACIÓN: ¿quién es? Lo dice el token del encabezado
        //    Authorization: Bearer <token>, si su firma, su emisor, su audiencia
        //    y su vencimiento son válidos.
        servicios
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(jwt =>
            {
                jwt.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = opciones.Emisor,
                    ValidateAudience = true,
                    ValidAudience = opciones.Audiencia,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(opciones.Secreto)),
                    RoleClaimType = ClaimTypes.Role,
                    // Por defecto se toleran 5 minutos de desfase entre relojes;
                    // con 30 segundos, un token vencido deja de servir de verdad.
                    ClockSkew = TimeSpan.FromSeconds(30)
                };

                // El 401 y el 403 también son contrato (Artículo 9): mismo sobre
                // {estado, mensaje, detalle} que los demás errores, en español.
                jwt.Events = new JwtBearerEvents
                {
                    OnChallenge = async contexto =>
                    {
                        contexto.HandleResponse();
                        var detalle = contexto.AuthenticateFailure is SecurityTokenExpiredException
                            ? "El token expiró. Vuelva a iniciar sesión."
                            : string.IsNullOrEmpty(contexto.Request.Headers.Authorization)
                                ? "Falta el token. Inicie sesión en POST /api/login y envíelo en Authorization: Bearer <token>."
                                : "El token no es válido.";
                        contexto.Response.StatusCode = 401;
                        await contexto.Response.WriteAsJsonAsync(new { estado = 401, mensaje = "No autenticado.", detalle });
                    },
                    OnForbidden = async contexto =>
                    {
                        contexto.Response.StatusCode = 403;
                        await contexto.Response.WriteAsJsonAsync(new
                        {
                            estado = 403,
                            mensaje = "Sin permiso.",
                            detalle = "Su rol no permite esta operación."
                        });
                    }
                };
            });

        // 4. AUTORIZACIÓN: ¿puede hacer ESTO? La regla por verbo (PermisoPorVerbo)
        //    como filtro GLOBAL de MVC: aplica a los 25 controladores sin que
        //    ninguno tenga que acordarse de pedirla. /api/login la esquiva con
        //    [AllowAnonymous]; usuario, rol y rol_usuario le suman su [Authorize].
        servicios.AddHttpContextAccessor();
        servicios.AddScoped<IAuthorizationHandler, PermisoPorVerboManejador>();
        servicios.AddAuthorization(o => o.AddPolicy(PermisoPorVerboRequisito.Politica, p => p
            .RequireAuthenticatedUser()
            .AddRequirements(new PermisoPorVerboRequisito())));
        servicios.Configure<Microsoft.AspNetCore.Mvc.MvcOptions>(o =>
            o.Filters.Add(new AuthorizeFilter(PermisoPorVerboRequisito.Politica)));

        // 5. Swagger con el botón «Authorize»: para probar desde /swagger se pega
        //    ahí el token que devolvió /api/login.
        servicios.ConfigureSwaggerGen(swagger =>
        {
            swagger.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "Pegue el token de POST /api/login (sin la palabra Bearer).",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT"
            });
            swagger.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                    },
                    Array.Empty<string>()
                }
            });
        });

        return servicios;
    }

    public static async Task UsarSeguridad(this WebApplication app)
    {
        // El orden importa: primero se averigua QUIÉN es, después QUÉ puede.
        app.UseAuthentication();
        app.UseAuthorization();

        await AsegurarAdministradorInicial(app);
    }

    /// <summary>
    /// Sin un primer administrador nadie podría ingresar a crear los demás. Si no
    /// hay ninguno activo, se crea con los datos del .env (ADMIN_USUARIO,
    /// ADMIN_CORREO, ADMIN_CLAVE). Si ya hay uno, no se toca nada.
    /// </summary>
    private static async Task AsegurarAdministradorInicial(WebApplication app)
    {
        var usuario = app.Configuration["AdminInicial:Usuario"];
        var correo = app.Configuration["AdminInicial:Correo"];
        var clave = app.Configuration["AdminInicial:Clave"];

        try
        {
            using var alcance = app.Services.CreateScope();
            var servicio = alcance.ServiceProvider.GetRequiredService<IServicioUsuario>();

            if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrWhiteSpace(correo) || string.IsNullOrWhiteSpace(clave))
            {
                app.Logger.LogWarning("No hay AdminInicial configurado (ADMIN_USUARIO, ADMIN_CORREO, ADMIN_CLAVE en .env): " +
                                      "si no existe ningún administrador, nadie podrá ingresar.");
                return;
            }

            await servicio.AsegurarAdministradorInicial(usuario, correo, clave);
        }
        catch (Exception ex)
        {
            // No se tumba la API por esto: el diagnóstico y Swagger siguen
            // respondiendo, y el error queda en el log para corregirlo.
            app.Logger.LogError(ex, "No se pudo asegurar el administrador inicial.");
        }
    }
}
