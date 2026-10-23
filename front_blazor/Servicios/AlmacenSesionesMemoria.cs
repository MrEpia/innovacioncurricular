using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Caching.Memory;

namespace FrontInnovacion.Servicios;

/// <summary>
/// LA SESIÓN DEL SERVIDOR (2_spec.md de la v3, RF7 · D-v3-7).
///
/// Por defecto, la autenticación por cookie mete el «ticket» completo —quién es,
/// sus roles y aquí también el JWT— cifrado DENTRO de la cookie, y el navegador
/// lo devuelve en cada petición. Con este almacén, el ticket se queda en la
/// memoria del servidor del front y la cookie solo lleva una llave opaca que lo
/// identifica. El JWT nunca sale del servidor.
///
/// Precio: si el contenedor del front se reinicia, las sesiones se pierden y
/// todos vuelven a ingresar. Para un único servidor, es aceptable.
/// </summary>
public class AlmacenSesionesMemoria : ITicketStore
{
    private readonly IMemoryCache _memoria;

    public AlmacenSesionesMemoria(IMemoryCache memoria)
    {
        _memoria = memoria;
    }

    public async Task<string> StoreAsync(AuthenticationTicket ticket)
    {
        var llave = "sesion-" + Guid.NewGuid().ToString("N");
        await RenewAsync(llave, ticket);
        return llave;
    }

    public Task RenewAsync(string key, AuthenticationTicket ticket)
    {
        var opciones = new MemoryCacheEntryOptions();

        // La sesión muere cuando muere el token: ni un minuto después.
        if (ticket.Properties.ExpiresUtc is { } vence)
        {
            opciones.SetAbsoluteExpiration(vence);
        }

        _memoria.Set(key, ticket, opciones);
        return Task.CompletedTask;
    }

    public Task<AuthenticationTicket?> RetrieveAsync(string key) =>
        Task.FromResult(_memoria.TryGetValue(key, out AuthenticationTicket? ticket) ? ticket : null);

    /// <summary>Cerrar sesión: el ticket —y con él el JWT— desaparece del servidor.</summary>
    public Task RemoveAsync(string key)
    {
        _memoria.Remove(key);
        return Task.CompletedTask;
    }
}
