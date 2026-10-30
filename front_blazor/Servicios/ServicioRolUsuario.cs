namespace FrontInnovacion.Servicios;

// Panel de administración (v3) — David Epia. Habla con la API por
// ClienteApi, que ya le pone el token; si quien lo usa no es administrador,
// la API responde 403 aunque alguien llegue a la pantalla saltándose el menú.

public class AsignacionRol
{
    public int UsuarioId { get; set; }
    public int RolId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
}

public class ServicioRolUsuario
{
    private readonly ClienteApi _api;

    public ServicioRolUsuario(ClienteApi api)
    {
        _api = api;
    }

    public Task<Resultado<List<AsignacionRol>>> Listar() =>
        _api.Listar<AsignacionRol>("/api/rol_usuario?limite=1000");

    public Task<Resultado<bool>> Asignar(int usuarioId, int rolId) =>
        _api.Enviar(HttpMethod.Post, "/api/rol_usuario", new { usuarioId, rolId });

    public Task<Resultado<bool>> Quitar(int usuarioId, int rolId) =>
        _api.Enviar(HttpMethod.Delete, $"/api/rol_usuario/{usuarioId}/{rolId}", null);
}
