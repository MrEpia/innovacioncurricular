namespace FrontInnovacion.Servicios;

/// <summary>
/// alianza, tal como el front lo maneja (v2 — bloque Actividades). Es una
/// clase del front, no de la API: se parece porque el contrato es el mismo.
///
/// Los números y las fechas son anulables para que un formulario a medio llenar
/// llegue a la API como «falta este dato» (422) y no como un cero inventado.
/// </summary>
public class Alianza
{
    /// <summary>Aliado</summary>
    public int? Aliado { get; set; }

    /// <summary>Departamento (programa)</summary>
    public int? Departamento { get; set; }

    /// <summary>Fecha de inicio</summary>
    public DateTime? FechaInicio { get; set; }

    /// <summary>Fecha de fin</summary>
    public DateTime? FechaFin { get; set; }

    /// <summary>Docente responsable (documento)</summary>
    public int? Docente { get; set; }
}

/// <summary>
/// La capa de datos del front para alianza: la única pieza que sabe en qué
/// ruta vive este recurso. El transporte lo pone ClienteApi (D-v2-4).
/// </summary>
public class ServicioAlianza
{
    private readonly ClienteApi _api;

    public ServicioAlianza(ClienteApi api)
    {
        _api = api;
    }

    // RF1 — Listar
    public Task<Resultado<List<Alianza>>> Listar(int limite = 1000) =>
        _api.Listar<Alianza>($"/api/alianza?limite={limite}");

    // RF2 — Obtener uno
    public Task<Resultado<Alianza>> Obtener(int aliado, int departamento) =>
        _api.Obtener<Alianza>($"/api/alianza/{aliado}/{departamento}");

    // RF3 — Crear: viaja la ficha entera, llave incluida.
    public Task<Resultado<bool>> Crear(Alianza entidad) =>
        _api.Enviar(HttpMethod.Post, "/api/alianza", entidad);

    // RF4 — Reemplazar: «guardar la ficha completa». La llave no va en el
    // cuerpo: identifica la fila y viaja en la ruta.
    public Task<Resultado<bool>> Reemplazar(int aliado, int departamento, Alianza entidad) =>
        _api.Enviar(HttpMethod.Put, $"/api/alianza/{aliado}/{departamento}", new { fechaInicio = entidad.FechaInicio, fechaFin = entidad.FechaFin, docente = entidad.Docente });

    // RF5 — Actualizar: «guardar solo lo que cambié». Viaja únicamente lo que
    // es distinto de lo que se cargó al abrir la edición (D-v2-10).
    public Task<Resultado<bool>> Actualizar(int aliado, int departamento, Alianza original, Alianza editado)
    {
        var cuerpo = new Dictionary<string, object?>();
        if (!Equals(original.FechaInicio, editado.FechaInicio)) cuerpo["fechaInicio"] = editado.FechaInicio;
        if (!Equals(original.FechaFin, editado.FechaFin)) cuerpo["fechaFin"] = editado.FechaFin;
        if (!Equals(original.Docente, editado.Docente)) cuerpo["docente"] = editado.Docente;

        if (cuerpo.Count == 0)
        {
            return Task.FromResult(Resultado<bool>.Mal(new() { "No cambió ningún dato." }));
        }

        return _api.Enviar(HttpMethod.Patch, $"/api/alianza/{aliado}/{departamento}", cuerpo);
    }

    // RF6 — Retirar del uso (la API lo hace lógico: la fila no se borra)
    public Task<Resultado<bool>> Eliminar(int aliado, int departamento) =>
        _api.Enviar(HttpMethod.Delete, $"/api/alianza/{aliado}/{departamento}", null);
}
