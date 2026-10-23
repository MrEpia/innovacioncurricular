namespace FrontInnovacion.Servicios;

/// <summary>
/// acreditacion, tal como el front lo maneja (v2 — bloque Academico). Es una
/// clase del front, no de la API: se parece porque el contrato es el mismo.
///
/// Los números y las fechas son anulables para que un formulario a medio llenar
/// llegue a la API como «falta este dato» (422) y no como un cero inventado.
/// </summary>
public class Acreditacion
{
    /// <summary>Resolución</summary>
    public int? Resolucion { get; set; }

    /// <summary>Tipo</summary>
    public string Tipo { get; set; } = string.Empty;

    /// <summary>Calificación</summary>
    public string Calificacion { get; set; } = string.Empty;

    /// <summary>Fecha de inicio</summary>
    public string FechaInicio { get; set; } = string.Empty;

    /// <summary>Fecha de fin</summary>
    public string FechaFin { get; set; } = string.Empty;

    /// <summary>Programa</summary>
    public int? Programa { get; set; }
}

/// <summary>
/// La capa de datos del front para acreditacion: la única pieza que sabe en qué
/// ruta vive este recurso. El transporte lo pone ClienteApi (D-v2-4).
/// </summary>
public class ServicioAcreditacion
{
    private readonly ClienteApi _api;

    public ServicioAcreditacion(ClienteApi api)
    {
        _api = api;
    }

    // RF1 — Listar
    public Task<Resultado<List<Acreditacion>>> Listar(int limite = 1000) =>
        _api.Listar<Acreditacion>($"/api/acreditacion?limite={limite}");

    // RF2 — Obtener uno
    public Task<Resultado<Acreditacion>> Obtener(int resolucion) =>
        _api.Obtener<Acreditacion>($"/api/acreditacion/{resolucion}");

    // RF3 — Crear: viaja la ficha entera, llave incluida.
    public Task<Resultado<bool>> Crear(Acreditacion entidad) =>
        _api.Enviar(HttpMethod.Post, "/api/acreditacion", entidad);

    // RF4 — Reemplazar: «guardar la ficha completa». La llave no va en el
    // cuerpo: identifica la fila y viaja en la ruta.
    public Task<Resultado<bool>> Reemplazar(int resolucion, Acreditacion entidad) =>
        _api.Enviar(HttpMethod.Put, $"/api/acreditacion/{resolucion}", new { tipo = entidad.Tipo, calificacion = entidad.Calificacion, fechaInicio = entidad.FechaInicio, fechaFin = entidad.FechaFin, programa = entidad.Programa });

    // RF5 — Actualizar: «guardar solo lo que cambié». Viaja únicamente lo que
    // es distinto de lo que se cargó al abrir la edición (D-v2-10).
    public Task<Resultado<bool>> Actualizar(int resolucion, Acreditacion original, Acreditacion editado)
    {
        var cuerpo = new Dictionary<string, object?>();
        if (!Equals(original.Tipo, editado.Tipo)) cuerpo["tipo"] = editado.Tipo;
        if (!Equals(original.Calificacion, editado.Calificacion)) cuerpo["calificacion"] = editado.Calificacion;
        if (!Equals(original.FechaInicio, editado.FechaInicio)) cuerpo["fechaInicio"] = editado.FechaInicio;
        if (!Equals(original.FechaFin, editado.FechaFin)) cuerpo["fechaFin"] = editado.FechaFin;
        if (!Equals(original.Programa, editado.Programa)) cuerpo["programa"] = editado.Programa;

        if (cuerpo.Count == 0)
        {
            return Task.FromResult(Resultado<bool>.Mal(new() { "No cambió ningún dato." }));
        }

        return _api.Enviar(HttpMethod.Patch, $"/api/acreditacion/{resolucion}", cuerpo);
    }

    // RF6 — Retirar del uso (la API lo hace lógico: la fila no se borra)
    public Task<Resultado<bool>> Eliminar(int resolucion) =>
        _api.Enviar(HttpMethod.Delete, $"/api/acreditacion/{resolucion}", null);
}
