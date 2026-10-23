namespace ApiInnovacion.Seguridad;

/// <summary>
/// La configuración del JWT. Llega por variables de entorno (Jwt__Secreto,
/// Jwt__Minutos), que el compose arma con el .env: el secreto de firma NUNCA
/// está escrito en el código ni en un archivo versionado (Artículo 7).
/// </summary>
public class OpcionesJwt
{
    /// <summary>La llave con la que se FIRMAN los tokens. Quien la conozca puede fabricarlos.</summary>
    public string Secreto { get; set; } = string.Empty;

    /// <summary>Cuántos minutos vive un token antes de expirar.</summary>
    public int Minutos { get; set; } = 60;

    /// <summary>Quién emite el token: la API. Se valida al recibirlo.</summary>
    public string Emisor { get; set; } = "innovacion-api";

    /// <summary>Para quién es el token: el front. Se valida al recibirlo.</summary>
    public string Audiencia { get; set; } = "innovacion-front";

    /// <summary>
    /// Falla AL ARRANCAR si la configuración no sirve, en vez de emitir tokens
    /// débiles: con HMAC-SHA256 una llave de menos de 32 bytes es atacable por
    /// fuerza bruta.
    /// </summary>
    public void Validar()
    {
        if (string.IsNullOrWhiteSpace(Secreto) || Secreto.Length < 32)
        {
            throw new InvalidOperationException(
                "Falta Jwt__Secreto o tiene menos de 32 caracteres. Defínalo en el .env (JWT_SECRETO).");
        }

        if (Minutos <= 0)
        {
            throw new InvalidOperationException("Jwt__Minutos debe ser mayor a 0.");
        }
    }
}
