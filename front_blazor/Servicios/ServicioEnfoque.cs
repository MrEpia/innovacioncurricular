namespace FrontInnovacion.Servicios;

/// <summary>
/// Enfoque, tal como el front lo maneja. Es una clase del front, no de la API:
/// se parece a la de allá porque el contrato es el mismo, pero son dos clases
/// distintas en dos proyectos distintos.
/// </summary>
public class Enfoque
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
}

/// <summary>
/// La capa de datos del front para enfoque: la única pieza que sabe dónde
/// viven los datos —en la API, nunca en la base de datos— y la única que
/// habla HTTP para este recurso.
/// </summary>
public class ServicioEnfoque
{
    // El transporte HTTP (sobres, 204, errores, API caída) vive en ClienteApi
    // desde la v2. Este servicio sigue siendo el que sabe QUÉ recurso es, qué
    // ruta lleva y qué campos tiene.
    private readonly ClienteApi _api;

    public ServicioEnfoque(ClienteApi api)
    {
        _api = api;
    }

    // RF1 — Listar
    public Task<Resultado<List<Enfoque>>> Listar(int limite = 1000) =>
        _api.Listar<Enfoque>($"/api/enfoque?limite={limite}");

    // RF2 — Obtener uno
    public Task<Resultado<Enfoque>> Obtener(int id) =>
        _api.Obtener<Enfoque>($"/api/enfoque/{id}");

    // RF3 — Crear
    public Task<Resultado<bool>> Crear(Enfoque entidad) =>
        _api.Enviar(HttpMethod.Post, "/api/enfoque", entidad);

    // RF4 — Reemplazar: «guardar la ficha completa». La llave no va en el
    // cuerpo: identifica la fila y viaja en la ruta.
    public Task<Resultado<bool>> Reemplazar(int id, Enfoque entidad) =>
        _api.Enviar(HttpMethod.Put, $"/api/enfoque/{id}", new { nombre = entidad.Nombre, descripcion = entidad.Descripcion });

    // RF5 — Actualizar: «guardar solo lo que cambié». Solo viaja lo diligenciado.
    public Task<Resultado<bool>> Actualizar(int id, string? nombre, string? descripcion)
    {
        var cuerpo = new Dictionary<string, object?>();
        if (!string.IsNullOrWhiteSpace(nombre)) cuerpo["nombre"] = nombre;
        if (!string.IsNullOrWhiteSpace(descripcion)) cuerpo["descripcion"] = descripcion;

        return _api.Enviar(HttpMethod.Patch, $"/api/enfoque/{id}", cuerpo);
    }

    // RF6 — Retirar del uso (la API lo hace lógico: la fila no se borra)
    public Task<Resultado<bool>> Eliminar(int id) =>
        _api.Enviar(HttpMethod.Delete, $"/api/enfoque/{id}", null);
}
