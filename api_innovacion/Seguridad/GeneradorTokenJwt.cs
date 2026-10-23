using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ApiInnovacion.Seguridad;

/// <summary>
/// El JWT tiene tres partes —cabecera, contenido y firma— separadas por puntos.
/// El contenido NO está cifrado: cualquiera puede leerlo en jwt.io. Por eso
/// aquí va solo lo necesario para autorizar (quién es y qué roles tiene) y
/// nunca la contraseña ni datos sensibles. Lo que lo protege es la FIRMA:
/// cambiar un rol dentro del token invalida la firma y la API lo rechaza.
/// </summary>
public class GeneradorTokenJwt : IGeneradorToken
{
    private readonly OpcionesJwt _opciones;

    public GeneradorTokenJwt(IOptions<OpcionesJwt> opciones)
    {
        _opciones = opciones.Value;
    }

    public TokenEmitido Generar(int usuarioId, string username, string? nombreCompleto, IEnumerable<string> roles)
    {
        var expira = DateTime.UtcNow.AddMinutes(_opciones.Minutos);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuarioId.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, username),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };
        if (!string.IsNullOrWhiteSpace(nombreCompleto))
        {
            claims.Add(new Claim(JwtRegisteredClaimNames.Name, nombreCompleto));
        }

        // Un claim "role" por cada rol, con el nombre CORTO del estándar. Si se
        // usara ClaimTypes.Role, el token llevaría el nombre largo de .NET
        // (http://schemas.microsoft.com/…/role) y otro cliente no lo
        // reconocería. Al validar, ASP.NET lo traduce de vuelta a ClaimTypes.Role,
        // que es lo que lee [Authorize(Roles = ...)].
        claims.AddRange(roles.Select(r => new Claim("role", r)));

        var llave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opciones.Secreto));
        var token = new JwtSecurityToken(
            issuer: _opciones.Emisor,
            audience: _opciones.Audiencia,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expira,
            signingCredentials: new SigningCredentials(llave, SecurityAlgorithms.HmacSha256));

        return new TokenEmitido(new JwtSecurityTokenHandler().WriteToken(token), expira);
    }
}
