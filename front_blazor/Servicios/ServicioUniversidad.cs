namespace FrontInnovacion.Servicios;

/// <summary>
/// Universidad, tal como el front lo maneja. Es una clase del front, no de la API:
/// se parece a la de allá porque el contrato es el mismo, pero son dos clases
/// distintas en dos proyectos distintos.
/// </summary>
public class Universidad
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public string Ciudad { get; set; } = string.Empty;
}

/// <summary>
/// La capa de datos del front para universidad: la única pieza que sabe dónde
/// viven los datos —en la API, nunca en la base de datos— y la única que
/// habla HTTP para este recurso.
/// </summary>
public class ServicioUniversidad
{
    // El transporte HTTP (sobres, 204, errores, API caída) vive en ClienteApi
    // desde la v2. Este servicio sigue siendo el que sabe QUÉ recurso es, qué
    // ruta lleva y qué campos tiene.
    private readonly ClienteApi _api;

    public ServicioUniversidad(ClienteApi api)
    {
        _api = api;
    }

    // RF1 — Listar
    public Task<Resultado<List<Universidad>>> Listar(int limite = 1000) =>
        _api.Listar<Universidad>($"/api/universidad?limite={limite}");

    // RF2 — Obtener uno
    public Task<Resultado<Universidad>> Obtener(int id) =>
        _api.Obtener<Universidad>($"/api/universidad/{id}");

    // RF3 — Crear
    public Task<Resultado<bool>> Crear(Universidad entidad) =>
        _api.Enviar(HttpMethod.Post, "/api/universidad", entidad);

    // RF4 — Reemplazar: «guardar la ficha completa». La llave no va en el
    // cuerpo: identifica la fila y viaja en la ruta.
    public Task<Resultado<bool>> Reemplazar(int id, Universidad entidad) =>
        _api.Enviar(HttpMethod.Put, $"/api/universidad/{id}", new { nombre = entidad.Nombre, tipo = entidad.Tipo, ciudad = entidad.Ciudad });

    // RF5 — Actualizar: «guardar solo lo que cambié». Solo viaja lo diligenciado.
    public Task<Resultado<bool>> Actualizar(int id, string? nombre, string? tipo, string? ciudad)
    {
        var cuerpo = new Dictionary<string, object?>();
        if (!string.IsNullOrWhiteSpace(nombre)) cuerpo["nombre"] = nombre;
        if (!string.IsNullOrWhiteSpace(tipo)) cuerpo["tipo"] = tipo;
        if (!string.IsNullOrWhiteSpace(ciudad)) cuerpo["ciudad"] = ciudad;

        return _api.Enviar(HttpMethod.Patch, $"/api/universidad/{id}", cuerpo);
    }

    // RF6 — Retirar del uso (la API lo hace lógico: la fila no se borra)
    public Task<Resultado<bool>> Eliminar(int id) =>
        _api.Enviar(HttpMethod.Delete, $"/api/universidad/{id}", null);
}
