using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace FrontInnovacion.Servicios;

/// <summary>Lo que devuelve POST /api/login, tal como el front lo usa.</summary>
public class Ingreso
{
    public string Token { get; set; } = string.Empty;
    public DateTime Expira { get; set; }
    public UsuarioIngreso Usuario { get; set; } = new();
}

public class UsuarioIngreso
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? NombreCompleto { get; set; }
    public List<string> Roles { get; set; } = new();
}

/// <summary>
/// El ingreso: le pregunta a la API si las credenciales sirven. No guarda nada:
/// guardar la sesión es trabajo de la página /ingresar, que es la única que
/// tiene la petición HTTP del navegador para dejarle la cookie.
///
/// No usa ClienteApi a propósito: ClienteApi le pone el token a todo, y aquí
/// todavía no hay token.
/// </summary>
public class ServicioSesion
{
    private readonly HttpClient _http;

    private static readonly JsonSerializerOptions _opciones = new() { PropertyNameCaseInsensitive = true };

    public ServicioSesion(HttpClient http)
    {
        _http = http;
    }

    public async Task<Resultado<Ingreso>> IniciarSesion(string usuario, string clave)
    {
        try
        {
            var r = await _http.PostAsJsonAsync("/api/login", new { usuario, clave });
            if (r.IsSuccessStatusCode)
            {
                var ingreso = await r.Content.ReadFromJsonAsync<Ingreso>(_opciones);
                return Resultado<Ingreso>.Bien(ingreso!);
            }

            // 401 y 403 traen su explicación en "detalle": «Usuario o contraseña
            // incorrectos», «no tiene ningún rol activo».
            var sobre = await r.Content.ReadFromJsonAsync<JsonElement>();
            var detalle = sobre.TryGetProperty("detalle", out var d) ? d.GetString() : null;
            return Resultado<Ingreso>.Mal(new()
            {
                detalle ?? (r.StatusCode == HttpStatusCode.UnprocessableEntity
                    ? "Escriba el usuario y la contraseña."
                    : "No se pudo iniciar sesión.")
            });
        }
        catch (Exception e) when (e is HttpRequestException or TaskCanceledException)
        {
            return Resultado<Ingreso>.Mal(new() { "El servicio no está disponible. ¿Está arriba la API?" });
        }
    }
}
