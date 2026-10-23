namespace FrontInnovacion.Servicios;

/// <summary>
/// activ_academica, tal como el front lo maneja (v2 — bloque Actividades). Es una
/// clase del front, no de la API: se parece porque el contrato es el mismo.
///
/// Los números y las fechas son anulables para que un formulario a medio llenar
/// llegue a la API como «falta este dato» (422) y no como un cero inventado.
/// </summary>
public class ActivAcademica
{
    /// <summary>ID</summary>
    public int? Id { get; set; }

    /// <summary>Nombre</summary>
    public string Nombre { get; set; } = string.Empty;

    /// <summary>Créditos</summary>
    public int? NumCreditos { get; set; }

    /// <summary>Tipo</summary>
    public string Tipo { get; set; } = string.Empty;

    /// <summary>Área de formación</summary>
    public string AreaFormacion { get; set; } = string.Empty;

    /// <summary>Horas de acompañamiento</summary>
    public int? HAcom { get; set; }

    /// <summary>Horas independientes</summary>
    public int? HIndep { get; set; }

    /// <summary>Idioma</summary>
    public string Idioma { get; set; } = string.Empty;

    /// <summary>¿Es espejo?</summary>
    public int? Espejo { get; set; }

    /// <summary>Entidad espejo</summary>
    public string EntidadEspejo { get; set; } = string.Empty;

    /// <summary>País espejo</summary>
    public string PaisEspejo { get; set; } = string.Empty;

    /// <summary>Programa (diseño)</summary>
    public int? Disenio { get; set; }
}

/// <summary>
/// La capa de datos del front para activ_academica: la única pieza que sabe en qué
/// ruta vive este recurso. El transporte lo pone ClienteApi (D-v2-4).
/// </summary>
public class ServicioActivAcademica
{
    private readonly ClienteApi _api;

    public ServicioActivAcademica(ClienteApi api)
    {
        _api = api;
    }

    // RF1 — Listar
    public Task<Resultado<List<ActivAcademica>>> Listar(int limite = 1000) =>
        _api.Listar<ActivAcademica>($"/api/activ_academica?limite={limite}");

    // RF2 — Obtener uno
    public Task<Resultado<ActivAcademica>> Obtener(int id) =>
        _api.Obtener<ActivAcademica>($"/api/activ_academica/{id}");

    // RF3 — Crear: viaja la ficha entera, llave incluida.
    public Task<Resultado<bool>> Crear(ActivAcademica entidad) =>
        _api.Enviar(HttpMethod.Post, "/api/activ_academica", entidad);

    // RF4 — Reemplazar: «guardar la ficha completa». La llave no va en el
    // cuerpo: identifica la fila y viaja en la ruta.
    public Task<Resultado<bool>> Reemplazar(int id, ActivAcademica entidad) =>
        _api.Enviar(HttpMethod.Put, $"/api/activ_academica/{id}", new { nombre = entidad.Nombre, numCreditos = entidad.NumCreditos, tipo = entidad.Tipo, areaFormacion = entidad.AreaFormacion, hAcom = entidad.HAcom, hIndep = entidad.HIndep, idioma = entidad.Idioma, espejo = entidad.Espejo, entidadEspejo = entidad.EntidadEspejo, paisEspejo = entidad.PaisEspejo, disenio = entidad.Disenio });

    // RF5 — Actualizar: «guardar solo lo que cambié». Viaja únicamente lo que
    // es distinto de lo que se cargó al abrir la edición (D-v2-10).
    public Task<Resultado<bool>> Actualizar(int id, ActivAcademica original, ActivAcademica editado)
    {
        var cuerpo = new Dictionary<string, object?>();
        if (!Equals(original.Nombre, editado.Nombre)) cuerpo["nombre"] = editado.Nombre;
        if (!Equals(original.NumCreditos, editado.NumCreditos)) cuerpo["numCreditos"] = editado.NumCreditos;
        if (!Equals(original.Tipo, editado.Tipo)) cuerpo["tipo"] = editado.Tipo;
        if (!Equals(original.AreaFormacion, editado.AreaFormacion)) cuerpo["areaFormacion"] = editado.AreaFormacion;
        if (!Equals(original.HAcom, editado.HAcom)) cuerpo["hAcom"] = editado.HAcom;
        if (!Equals(original.HIndep, editado.HIndep)) cuerpo["hIndep"] = editado.HIndep;
        if (!Equals(original.Idioma, editado.Idioma)) cuerpo["idioma"] = editado.Idioma;
        if (!Equals(original.Espejo, editado.Espejo)) cuerpo["espejo"] = editado.Espejo;
        if (!Equals(original.EntidadEspejo, editado.EntidadEspejo)) cuerpo["entidadEspejo"] = editado.EntidadEspejo;
        if (!Equals(original.PaisEspejo, editado.PaisEspejo)) cuerpo["paisEspejo"] = editado.PaisEspejo;
        if (!Equals(original.Disenio, editado.Disenio)) cuerpo["disenio"] = editado.Disenio;

        if (cuerpo.Count == 0)
        {
            return Task.FromResult(Resultado<bool>.Mal(new() { "No cambió ningún dato." }));
        }

        return _api.Enviar(HttpMethod.Patch, $"/api/activ_academica/{id}", cuerpo);
    }

    // RF6 — Retirar del uso (la API lo hace lógico: la fila no se borra)
    public Task<Resultado<bool>> Eliminar(int id) =>
        _api.Enviar(HttpMethod.Delete, $"/api/activ_academica/{id}", null);
}
