namespace FrontInnovacion.Servicios;

/// <summary>
/// premio, tal como el front lo maneja (v2 — bloque Actividades). Es una
/// clase del front, no de la API: se parece porque el contrato es el mismo.
///
/// Los números y las fechas son anulables para que un formulario a medio llenar
/// llegue a la API como «falta este dato» (422) y no como un cero inventado.
/// </summary>
public class Premio
{
    /// <summary>ID</summary>
    public int? Id { get; set; }

    /// <summary>Nombre</summary>
    public string Nombre { get; set; } = string.Empty;

    /// <summary>Descripción</summary>
    public string Descripcion { get; set; } = string.Empty;

    /// <summary>Fecha</summary>
    public DateTime? Fecha { get; set; }

    /// <summary>Entidad otorgante</summary>
    public string EntidadOtorgante { get; set; } = string.Empty;

    /// <summary>País</summary>
    public string Pais { get; set; } = string.Empty;

    /// <summary>Programa</summary>
    public int? Programa { get; set; }
}

/// <summary>
/// La capa de datos del front para premio: la única pieza que sabe en qué
/// ruta vive este recurso. El transporte lo pone ClienteApi (D-v2-4).
/// </summary>
public class ServicioPremio
{
    private readonly ClienteApi _api;

    public ServicioPremio(ClienteApi api)
    {
        _api = api;
    }

    // RF1 — Listar
    public Task<Resultado<List<Premio>>> Listar(int limite = 1000) =>
        _api.Listar<Premio>($"/api/premio?limite={limite}");

    // RF2 — Obtener uno
    public Task<Resultado<Premio>> Obtener(int id) =>
        _api.Obtener<Premio>($"/api/premio/{id}");

    // RF3 — Crear: viaja la ficha entera, llave incluida.
    public Task<Resultado<bool>> Crear(Premio entidad) =>
        _api.Enviar(HttpMethod.Post, "/api/premio", entidad);

    // RF4 — Reemplazar: «guardar la ficha completa». La llave no va en el
    // cuerpo: identifica la fila y viaja en la ruta.
    public Task<Resultado<bool>> Reemplazar(int id, Premio entidad) =>
        _api.Enviar(HttpMethod.Put, $"/api/premio/{id}", new { nombre = entidad.Nombre, descripcion = entidad.Descripcion, fecha = entidad.Fecha, entidadOtorgante = entidad.EntidadOtorgante, pais = entidad.Pais, programa = entidad.Programa });

    // RF5 — Actualizar: «guardar solo lo que cambié». Viaja únicamente lo que
    // es distinto de lo que se cargó al abrir la edición (D-v2-10).
    public Task<Resultado<bool>> Actualizar(int id, Premio original, Premio editado)
    {
        var cuerpo = new Dictionary<string, object?>();
        if (!Equals(original.Nombre, editado.Nombre)) cuerpo["nombre"] = editado.Nombre;
        if (!Equals(original.Descripcion, editado.Descripcion)) cuerpo["descripcion"] = editado.Descripcion;
        if (!Equals(original.Fecha, editado.Fecha)) cuerpo["fecha"] = editado.Fecha;
        if (!Equals(original.EntidadOtorgante, editado.EntidadOtorgante)) cuerpo["entidadOtorgante"] = editado.EntidadOtorgante;
        if (!Equals(original.Pais, editado.Pais)) cuerpo["pais"] = editado.Pais;
        if (!Equals(original.Programa, editado.Programa)) cuerpo["programa"] = editado.Programa;

        if (cuerpo.Count == 0)
        {
            return Task.FromResult(Resultado<bool>.Mal(new() { "No cambió ningún dato." }));
        }

        return _api.Enviar(HttpMethod.Patch, $"/api/premio/{id}", cuerpo);
    }

    // RF6 — Retirar del uso (la API lo hace lógico: la fila no se borra)
    public Task<Resultado<bool>> Eliminar(int id) =>
        _api.Enviar(HttpMethod.Delete, $"/api/premio/{id}", null);
}
