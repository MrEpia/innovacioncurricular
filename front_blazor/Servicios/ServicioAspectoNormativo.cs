namespace FrontInnovacion.Servicios;

/// <summary>
/// AspectoNormativo, tal como el front lo maneja. Es una clase del front, no de la API:
/// se parece a la de allá porque el contrato es el mismo, pero son dos clases
/// distintas en dos proyectos distintos.
/// </summary>
public class AspectoNormativo
{
    public int Id { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string Fuente { get; set; } = string.Empty;
}

/// <summary>
/// La capa de datos del front para aspecto_normativo: la única pieza que sabe dónde
/// viven los datos —en la API, nunca en la base de datos— y la única que
/// habla HTTP para este recurso.
/// </summary>
public class ServicioAspectoNormativo
{
    // El transporte HTTP (sobres, 204, errores, API caída) vive en ClienteApi
    // desde la v2. Este servicio sigue siendo el que sabe QUÉ recurso es, qué
    // ruta lleva y qué campos tiene.
    private readonly ClienteApi _api;

    public ServicioAspectoNormativo(ClienteApi api)
    {
        _api = api;
    }

    // RF1 — Listar
    public Task<Resultado<List<AspectoNormativo>>> Listar(int limite = 1000) =>
        _api.Listar<AspectoNormativo>($"/api/aspecto_normativo?limite={limite}");

    // RF2 — Obtener uno
    public Task<Resultado<AspectoNormativo>> Obtener(int id) =>
        _api.Obtener<AspectoNormativo>($"/api/aspecto_normativo/{id}");

    // RF3 — Crear
    public Task<Resultado<bool>> Crear(AspectoNormativo entidad) =>
        _api.Enviar(HttpMethod.Post, "/api/aspecto_normativo", entidad);

    // RF4 — Reemplazar: «guardar la ficha completa». La llave no va en el
    // cuerpo: identifica la fila y viaja en la ruta.
    public Task<Resultado<bool>> Reemplazar(int id, AspectoNormativo entidad) =>
        _api.Enviar(HttpMethod.Put, $"/api/aspecto_normativo/{id}", new { tipo = entidad.Tipo, descripcion = entidad.Descripcion, fuente = entidad.Fuente });

    // RF5 — Actualizar: «guardar solo lo que cambié». Solo viaja lo diligenciado.
    public Task<Resultado<bool>> Actualizar(int id, string? tipo, string? descripcion, string? fuente)
    {
        var cuerpo = new Dictionary<string, object?>();
        if (!string.IsNullOrWhiteSpace(tipo)) cuerpo["tipo"] = tipo;
        if (!string.IsNullOrWhiteSpace(descripcion)) cuerpo["descripcion"] = descripcion;
        if (!string.IsNullOrWhiteSpace(fuente)) cuerpo["fuente"] = fuente;

        return _api.Enviar(HttpMethod.Patch, $"/api/aspecto_normativo/{id}", cuerpo);
    }

    // RF6 — Retirar del uso (la API lo hace lógico: la fila no se borra)
    public Task<Resultado<bool>> Eliminar(int id) =>
        _api.Enviar(HttpMethod.Delete, $"/api/aspecto_normativo/{id}", null);
}
