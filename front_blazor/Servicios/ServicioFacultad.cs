namespace FrontInnovacion.Servicios;

/// <summary>
/// facultad, tal como el front lo maneja (v2 — bloque Academico). Es una
/// clase del front, no de la API: se parece porque el contrato es el mismo.
///
/// Los números y las fechas son anulables para que un formulario a medio llenar
/// llegue a la API como «falta este dato» (422) y no como un cero inventado.
/// </summary>
public class Facultad
{
    /// <summary>ID</summary>
    public int? Id { get; set; }

    /// <summary>Nombre</summary>
    public string Nombre { get; set; } = string.Empty;

    /// <summary>Tipo</summary>
    public string Tipo { get; set; } = string.Empty;

    /// <summary>Fecha de fundación</summary>
    public DateTime? FechaFun { get; set; }

    /// <summary>Universidad (sede)</summary>
    public int? Universidad { get; set; }
}

/// <summary>
/// La capa de datos del front para facultad: la única pieza que sabe en qué
/// ruta vive este recurso. El transporte lo pone ClienteApi (D-v2-4).
/// </summary>
public class ServicioFacultad
{
    private readonly ClienteApi _api;

    public ServicioFacultad(ClienteApi api)
    {
        _api = api;
    }

    // RF1 — Listar
    public Task<Resultado<List<Facultad>>> Listar(int limite = 1000) =>
        _api.Listar<Facultad>($"/api/facultad?limite={limite}");

    // RF2 — Obtener uno
    public Task<Resultado<Facultad>> Obtener(int id) =>
        _api.Obtener<Facultad>($"/api/facultad/{id}");

    // RF3 — Crear: viaja la ficha entera, llave incluida.
    public Task<Resultado<bool>> Crear(Facultad entidad) =>
        _api.Enviar(HttpMethod.Post, "/api/facultad", entidad);

    // RF4 — Reemplazar: «guardar la ficha completa». La llave no va en el
    // cuerpo: identifica la fila y viaja en la ruta.
    public Task<Resultado<bool>> Reemplazar(int id, Facultad entidad) =>
        _api.Enviar(HttpMethod.Put, $"/api/facultad/{id}", new { nombre = entidad.Nombre, tipo = entidad.Tipo, fechaFun = entidad.FechaFun, universidad = entidad.Universidad });

    // RF5 — Actualizar: «guardar solo lo que cambié». Viaja únicamente lo que
    // es distinto de lo que se cargó al abrir la edición (D-v2-10).
    public Task<Resultado<bool>> Actualizar(int id, Facultad original, Facultad editado)
    {
        var cuerpo = new Dictionary<string, object?>();
        if (!Equals(original.Nombre, editado.Nombre)) cuerpo["nombre"] = editado.Nombre;
        if (!Equals(original.Tipo, editado.Tipo)) cuerpo["tipo"] = editado.Tipo;
        if (!Equals(original.FechaFun, editado.FechaFun)) cuerpo["fechaFun"] = editado.FechaFun;
        if (!Equals(original.Universidad, editado.Universidad)) cuerpo["universidad"] = editado.Universidad;

        if (cuerpo.Count == 0)
        {
            return Task.FromResult(Resultado<bool>.Mal(new() { "No cambió ningún dato." }));
        }

        return _api.Enviar(HttpMethod.Patch, $"/api/facultad/{id}", cuerpo);
    }

    // RF6 — Retirar del uso (la API lo hace lógico: la fila no se borra)
    public Task<Resultado<bool>> Eliminar(int id) =>
        _api.Enviar(HttpMethod.Delete, $"/api/facultad/{id}", null);
}
