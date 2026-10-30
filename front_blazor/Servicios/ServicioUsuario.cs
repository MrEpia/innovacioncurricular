namespace FrontInnovacion.Servicios;

// Panel de administración (v3) — David Epia. Habla con la API por
// ClienteApi, que ya le pone el token; si quien lo usa no es administrador,
// la API responde 403 aunque alguien llegue a la pantalla saltándose el menú.

/// <summary>Un usuario del sistema, tal como lo devuelve /api/usuario (nunca trae la contraseña).</summary>
public class UsuarioSistema
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? NombreCompleto { get; set; }
    public List<string> Roles { get; set; } = new();
}

public class ServicioUsuario
{
    private readonly ClienteApi _api;

    public ServicioUsuario(ClienteApi api)
    {
        _api = api;
    }

    public Task<Resultado<List<UsuarioSistema>>> Listar() =>
        _api.Listar<UsuarioSistema>("/api/usuario?limite=1000");

    /// <summary>La clave viaja una sola vez, por HTTP hacia la API, que la hashea.</summary>
    public Task<Resultado<bool>> Crear(string username, string clave, string email, string? nombreCompleto) =>
        _api.Enviar(HttpMethod.Post, "/api/usuario", new { username, clave, email, nombreCompleto });

    /// <summary>«Guardar la ficha completa»: datos del usuario, sin la clave.</summary>
    public Task<Resultado<bool>> Reemplazar(int id, string username, string email, string? nombreCompleto) =>
        _api.Enviar(HttpMethod.Put, $"/api/usuario/{id}", new { username, email, nombreCompleto });

    /// <summary>Cambiar la contraseña es un PATCH con solo la clave.</summary>
    public Task<Resultado<bool>> CambiarClave(int id, string clave) =>
        _api.Enviar(HttpMethod.Patch, $"/api/usuario/{id}", new { clave });

    public Task<Resultado<bool>> Eliminar(int id) =>
        _api.Enviar(HttpMethod.Delete, $"/api/usuario/{id}", null);
}
