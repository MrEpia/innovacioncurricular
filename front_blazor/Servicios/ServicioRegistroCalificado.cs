namespace FrontInnovacion.Servicios;

/// <summary>
/// registro_calificado, tal como el front lo maneja (v2 — bloque Academico). Es una
/// clase del front, no de la API: se parece porque el contrato es el mismo.
///
/// Los números y las fechas son anulables para que un formulario a medio llenar
/// llegue a la API como «falta este dato» (422) y no como un cero inventado.
/// </summary>
public class RegistroCalificado
{
    /// <summary>Código</summary>
    public int? Codigo { get; set; }

    /// <summary>Créditos</summary>
    public string CantCreditos { get; set; } = string.Empty;

    /// <summary>Horas de acompañamiento</summary>
    public string HoraAcom { get; set; } = string.Empty;

    /// <summary>Horas independientes</summary>
    public string HoraInd { get; set; } = string.Empty;

    /// <summary>Metodología</summary>
    public string Metodologia { get; set; } = string.Empty;

    /// <summary>Fecha de inicio</summary>
    public DateTime? FechaInicio { get; set; }

    /// <summary>Fecha de fin</summary>
    public DateTime? FechaFin { get; set; }

    /// <summary>Duración (años)</summary>
    public string DuracionAnios { get; set; } = string.Empty;

    /// <summary>Duración (semestres)</summary>
    public string DuracionSemestres { get; set; } = string.Empty;

    /// <summary>Tipo de titulación</summary>
    public string TipoTitulacion { get; set; } = string.Empty;

    /// <summary>Programa</summary>
    public int? Programa { get; set; }
}

/// <summary>
/// La capa de datos del front para registro_calificado: la única pieza que sabe en qué
/// ruta vive este recurso. El transporte lo pone ClienteApi (D-v2-4).
/// </summary>
public class ServicioRegistroCalificado
{
    private readonly ClienteApi _api;

    public ServicioRegistroCalificado(ClienteApi api)
    {
        _api = api;
    }

    // RF1 — Listar
    public Task<Resultado<List<RegistroCalificado>>> Listar(int limite = 1000) =>
        _api.Listar<RegistroCalificado>($"/api/registro_calificado?limite={limite}");

    // RF2 — Obtener uno
    public Task<Resultado<RegistroCalificado>> Obtener(int codigo) =>
        _api.Obtener<RegistroCalificado>($"/api/registro_calificado/{codigo}");

    // RF3 — Crear: viaja la ficha entera, llave incluida.
    public Task<Resultado<bool>> Crear(RegistroCalificado entidad) =>
        _api.Enviar(HttpMethod.Post, "/api/registro_calificado", entidad);

    // RF4 — Reemplazar: «guardar la ficha completa». La llave no va en el
    // cuerpo: identifica la fila y viaja en la ruta.
    public Task<Resultado<bool>> Reemplazar(int codigo, RegistroCalificado entidad) =>
        _api.Enviar(HttpMethod.Put, $"/api/registro_calificado/{codigo}", new { cantCreditos = entidad.CantCreditos, horaAcom = entidad.HoraAcom, horaInd = entidad.HoraInd, metodologia = entidad.Metodologia, fechaInicio = entidad.FechaInicio, fechaFin = entidad.FechaFin, duracionAnios = entidad.DuracionAnios, duracionSemestres = entidad.DuracionSemestres, tipoTitulacion = entidad.TipoTitulacion, programa = entidad.Programa });

    // RF5 — Actualizar: «guardar solo lo que cambié». Viaja únicamente lo que
    // es distinto de lo que se cargó al abrir la edición (D-v2-10).
    public Task<Resultado<bool>> Actualizar(int codigo, RegistroCalificado original, RegistroCalificado editado)
    {
        var cuerpo = new Dictionary<string, object?>();
        if (!Equals(original.CantCreditos, editado.CantCreditos)) cuerpo["cantCreditos"] = editado.CantCreditos;
        if (!Equals(original.HoraAcom, editado.HoraAcom)) cuerpo["horaAcom"] = editado.HoraAcom;
        if (!Equals(original.HoraInd, editado.HoraInd)) cuerpo["horaInd"] = editado.HoraInd;
        if (!Equals(original.Metodologia, editado.Metodologia)) cuerpo["metodologia"] = editado.Metodologia;
        if (!Equals(original.FechaInicio, editado.FechaInicio)) cuerpo["fechaInicio"] = editado.FechaInicio;
        if (!Equals(original.FechaFin, editado.FechaFin)) cuerpo["fechaFin"] = editado.FechaFin;
        if (!Equals(original.DuracionAnios, editado.DuracionAnios)) cuerpo["duracionAnios"] = editado.DuracionAnios;
        if (!Equals(original.DuracionSemestres, editado.DuracionSemestres)) cuerpo["duracionSemestres"] = editado.DuracionSemestres;
        if (!Equals(original.TipoTitulacion, editado.TipoTitulacion)) cuerpo["tipoTitulacion"] = editado.TipoTitulacion;
        if (!Equals(original.Programa, editado.Programa)) cuerpo["programa"] = editado.Programa;

        if (cuerpo.Count == 0)
        {
            return Task.FromResult(Resultado<bool>.Mal(new() { "No cambió ningún dato." }));
        }

        return _api.Enviar(HttpMethod.Patch, $"/api/registro_calificado/{codigo}", cuerpo);
    }

    // RF6 — Retirar del uso (la API lo hace lógico: la fila no se borra)
    public Task<Resultado<bool>> Eliminar(int codigo) =>
        _api.Enviar(HttpMethod.Delete, $"/api/registro_calificado/{codigo}", null);
}
