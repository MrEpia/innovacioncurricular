namespace FrontInnovacion.Servicios;

/// <summary>
/// docente_departamento, tal como el front lo maneja (v2 — bloque Actividades). Es una
/// clase del front, no de la API: se parece porque el contrato es el mismo.
///
/// Los números y las fechas son anulables para que un formulario a medio llenar
/// llegue a la API como «falta este dato» (422) y no como un cero inventado.
/// </summary>
public class DocenteDepartamento
{
    /// <summary>Docente (documento)</summary>
    public int? Docente { get; set; }

    /// <summary>Departamento (programa)</summary>
    public int? Departamento { get; set; }

    /// <summary>Dedicación</summary>
    public string Dedicacion { get; set; } = string.Empty;

    /// <summary>Modalidad</summary>
    public string Modalidad { get; set; } = string.Empty;

    /// <summary>Fecha de ingreso</summary>
    public DateTime? FechaIngreso { get; set; }

    /// <summary>Fecha de salida</summary>
    public DateTime? FechaSalida { get; set; }
}

/// <summary>
/// La capa de datos del front para docente_departamento: la única pieza que sabe en qué
/// ruta vive este recurso. El transporte lo pone ClienteApi (D-v2-4).
/// </summary>
public class ServicioDocenteDepartamento
{
    private readonly ClienteApi _api;

    public ServicioDocenteDepartamento(ClienteApi api)
    {
        _api = api;
    }

    // RF1 — Listar
    public Task<Resultado<List<DocenteDepartamento>>> Listar(int limite = 1000) =>
        _api.Listar<DocenteDepartamento>($"/api/docente_departamento?limite={limite}");

    // RF2 — Obtener uno
    public Task<Resultado<DocenteDepartamento>> Obtener(int docente, int departamento) =>
        _api.Obtener<DocenteDepartamento>($"/api/docente_departamento/{docente}/{departamento}");

    // RF3 — Crear: viaja la ficha entera, llave incluida.
    public Task<Resultado<bool>> Crear(DocenteDepartamento entidad) =>
        _api.Enviar(HttpMethod.Post, "/api/docente_departamento", entidad);

    // RF4 — Reemplazar: «guardar la ficha completa». La llave no va en el
    // cuerpo: identifica la fila y viaja en la ruta.
    public Task<Resultado<bool>> Reemplazar(int docente, int departamento, DocenteDepartamento entidad) =>
        _api.Enviar(HttpMethod.Put, $"/api/docente_departamento/{docente}/{departamento}", new { dedicacion = entidad.Dedicacion, modalidad = entidad.Modalidad, fechaIngreso = entidad.FechaIngreso, fechaSalida = entidad.FechaSalida });

    // RF5 — Actualizar: «guardar solo lo que cambié». Viaja únicamente lo que
    // es distinto de lo que se cargó al abrir la edición (D-v2-10).
    public Task<Resultado<bool>> Actualizar(int docente, int departamento, DocenteDepartamento original, DocenteDepartamento editado)
    {
        var cuerpo = new Dictionary<string, object?>();
        if (!Equals(original.Dedicacion, editado.Dedicacion)) cuerpo["dedicacion"] = editado.Dedicacion;
        if (!Equals(original.Modalidad, editado.Modalidad)) cuerpo["modalidad"] = editado.Modalidad;
        if (!Equals(original.FechaIngreso, editado.FechaIngreso)) cuerpo["fechaIngreso"] = editado.FechaIngreso;
        if (!Equals(original.FechaSalida, editado.FechaSalida)) cuerpo["fechaSalida"] = editado.FechaSalida;

        if (cuerpo.Count == 0)
        {
            return Task.FromResult(Resultado<bool>.Mal(new() { "No cambió ningún dato." }));
        }

        return _api.Enviar(HttpMethod.Patch, $"/api/docente_departamento/{docente}/{departamento}", cuerpo);
    }

    // RF6 — Retirar del uso (la API lo hace lógico: la fila no se borra)
    public Task<Resultado<bool>> Eliminar(int docente, int departamento) =>
        _api.Enviar(HttpMethod.Delete, $"/api/docente_departamento/{docente}/{departamento}", null);
}
