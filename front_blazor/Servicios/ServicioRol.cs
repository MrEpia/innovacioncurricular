namespace FrontInnovacion.Servicios;

// Panel de administración (v3) — Sebastián Brunal. Habla con la API por
// ClienteApi, que ya le pone el token; si quien lo usa no es administrador,
// la API responde 403 aunque alguien llegue a la pantalla saltándose el menú.

public class RolSistema
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
}

public class ServicioRol
{
    private readonly ClienteApi _api;

    public ServicioRol(ClienteApi api)
    {
        _api = api;
    }

    public Task<Resultado<List<RolSistema>>> Listar() =>
        _api.Listar<RolSistema>("/api/rol?limite=1000");

    public Task<Resultado<bool>> Crear(string nombre, string? descripcion) =>
        _api.Enviar(HttpMethod.Post, "/api/rol", new { nombre, descripcion });

    public Task<Resultado<bool>> Reemplazar(int id, string nombre, string? descripcion) =>
        _api.Enviar(HttpMethod.Put, $"/api/rol/{id}", new { nombre, descripcion });

    public Task<Resultado<bool>> Eliminar(int id) =>
        _api.Enviar(HttpMethod.Delete, $"/api/rol/{id}", null);
}
