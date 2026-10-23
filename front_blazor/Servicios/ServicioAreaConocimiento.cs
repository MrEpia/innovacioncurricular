namespace FrontInnovacion.Servicios;

/// <summary>
/// AreaConocimiento, tal como el front lo maneja. Es una clase del front, no de la API:
/// se parece a la de allá porque el contrato es el mismo, pero son dos clases
/// distintas en dos proyectos distintos.
/// </summary>
public class AreaConocimiento
{
    public string Id { get; set; } = string.Empty;
    public string GranArea { get; set; } = string.Empty;
    public string Area { get; set; } = string.Empty;
    public string Disciplina { get; set; } = string.Empty;
}

/// <summary>
/// La capa de datos del front para area_conocimiento: la única pieza que sabe dónde
/// viven los datos —en la API, nunca en la base de datos— y la única que
/// habla HTTP para este recurso.
/// </summary>
public class ServicioAreaConocimiento
{
    // El transporte HTTP (sobres, 204, errores, API caída) vive en ClienteApi
    // desde la v2. Este servicio sigue siendo el que sabe QUÉ recurso es, qué
    // ruta lleva y qué campos tiene.
    private readonly ClienteApi _api;

    public ServicioAreaConocimiento(ClienteApi api)
    {
        _api = api;
    }

    // RF1 — Listar
    public Task<Resultado<List<AreaConocimiento>>> Listar(int limite = 1000) =>
        _api.Listar<AreaConocimiento>($"/api/area_conocimiento?limite={limite}");

    // RF2 — Obtener uno
    public Task<Resultado<AreaConocimiento>> Obtener(string id) =>
        _api.Obtener<AreaConocimiento>($"/api/area_conocimiento/{Uri.EscapeDataString(id)}");

    // RF3 — Crear
    public Task<Resultado<bool>> Crear(AreaConocimiento entidad) =>
        _api.Enviar(HttpMethod.Post, "/api/area_conocimiento", entidad);

    // RF4 — Reemplazar: «guardar la ficha completa». La llave no va en el
    // cuerpo: identifica la fila y viaja en la ruta.
    public Task<Resultado<bool>> Reemplazar(string id, AreaConocimiento entidad) =>
        _api.Enviar(HttpMethod.Put, $"/api/area_conocimiento/{Uri.EscapeDataString(id)}", new { granArea = entidad.GranArea, area = entidad.Area, disciplina = entidad.Disciplina });

    // RF5 — Actualizar: «guardar solo lo que cambié». Solo viaja lo diligenciado.
    public Task<Resultado<bool>> Actualizar(string id, string? granArea, string? area, string? disciplina)
    {
        var cuerpo = new Dictionary<string, object?>();
        if (!string.IsNullOrWhiteSpace(granArea)) cuerpo["granArea"] = granArea;
        if (!string.IsNullOrWhiteSpace(area)) cuerpo["area"] = area;
        if (!string.IsNullOrWhiteSpace(disciplina)) cuerpo["disciplina"] = disciplina;

        return _api.Enviar(HttpMethod.Patch, $"/api/area_conocimiento/{Uri.EscapeDataString(id)}", cuerpo);
    }

    // RF6 — Retirar del uso (la API lo hace lógico: la fila no se borra)
    public Task<Resultado<bool>> Eliminar(string id) =>
        _api.Enviar(HttpMethod.Delete, $"/api/area_conocimiento/{Uri.EscapeDataString(id)}", null);
}
