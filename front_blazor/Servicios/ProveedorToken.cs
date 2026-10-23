using Microsoft.AspNetCore.Components.Authorization;

namespace FrontInnovacion.Servicios;

/// <summary>
/// De dónde saca ClienteApi el JWT que manda a la API: del usuario de la sesión,
/// donde quedó guardado como el claim "jwt" al ingresar.
///
/// Funciona igual en las pantallas estáticas y en las interactivas: Blazor le
/// entrega a cada circuito el usuario de la petición que lo abrió.
/// </summary>
public class ProveedorToken
{
    public const string ClaimToken = "jwt";

    private readonly AuthenticationStateProvider _estado;

    public ProveedorToken(AuthenticationStateProvider estado)
    {
        _estado = estado;
    }

    public async Task<string?> Obtener()
    {
        var estado = await _estado.GetAuthenticationStateAsync();
        return estado.User.FindFirst(ClaimToken)?.Value;
    }
}
