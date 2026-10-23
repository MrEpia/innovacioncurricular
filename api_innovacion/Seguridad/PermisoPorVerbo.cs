using Microsoft.AspNetCore.Authorization;

namespace ApiInnovacion.Seguridad;

/// <summary>
/// La regla de permisos del módulo (2_spec.md de la v3, RF4), expresada por el
/// VERBO HTTP de la petición:
///
///   GET, HEAD             → administrador, editor, consulta
///   POST, PUT, PATCH      → administrador, editor
///   DELETE                → administrador
///
/// Se aplica a TODOS los controladores con un filtro global (Ensamblador-
/// Seguridad), no con un atributo en cada uno de los 22: así ninguna tabla
/// nueva puede olvidarse de pedirla. Un recurso que necesite algo más estricto
/// lo agrega encima —como usuario, rol y rol_usuario, que exigen además el rol
/// administrador— y ambas reglas tienen que cumplirse (decisión D-v3-4).
/// </summary>
public class PermisoPorVerboRequisito : IAuthorizationRequirement
{
    public const string Politica = "PermisoPorVerbo";
}

public class PermisoPorVerboManejador : AuthorizationHandler<PermisoPorVerboRequisito>
{
    private readonly IHttpContextAccessor _contexto;

    public PermisoPorVerboManejador(IHttpContextAccessor contexto)
    {
        _contexto = contexto;
    }

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext contexto, PermisoPorVerboRequisito requisito)
    {
        var metodo = _contexto.HttpContext?.Request.Method ?? string.Empty;

        string[] permitidos = metodo.ToUpperInvariant() switch
        {
            "GET" or "HEAD" => new[] { Roles.Administrador, Roles.Editor, Roles.Consulta },
            "POST" or "PUT" or "PATCH" => new[] { Roles.Administrador, Roles.Editor },
            "DELETE" => new[] { Roles.Administrador },
            // Cualquier otro verbo no tiene permiso para nadie: lo que no está
            // permitido explícitamente, está prohibido.
            _ => Array.Empty<string>()
        };

        if (permitidos.Any(rol => contexto.User.IsInRole(rol)))
        {
            contexto.Succeed(requisito);
        }

        // Si no se llama Succeed, la autorización falla: 401 si no hay token
        // válido, 403 si lo hay pero el rol no alcanza.
        return Task.CompletedTask;
    }
}
