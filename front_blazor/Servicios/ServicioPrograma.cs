namespace FrontInnovacion.Servicios;

/// <summary>
/// programa, tal como el front lo maneja (v2 — bloque Academico). Es una
/// clase del front, no de la API: se parece porque el contrato es el mismo.
///
/// Los números y las fechas son anulables para que un formulario a medio llenar
/// llegue a la API como «falta este dato» (422) y no como un cero inventado.
/// </summary>
public class Programa
{
    /// <summary>ID</summary>
    public int? Id { get; set; }

    /// <summary>Nombre</summary>
    public string Nombre { get; set; } = string.Empty;

    /// <summary>Tipo</summary>
    public string Tipo { get; set; } = string.Empty;

    /// <summary>Nivel</summary>
    public string Nivel { get; set; } = string.Empty;

    /// <summary>Fecha de creación</summary>
    public string FechaCreacion { get; set; } = string.Empty;

    /// <summary>Fecha de cierre</summary>
    public string? FechaCierre { get; set; }

    /// <summary>Número de cohortes</summary>
    public string NumeroCohortes { get; set; } = string.Empty;

    /// <summary>Cantidad de graduados</summary>
    public string CantGraduados { get; set; } = string.Empty;

    /// <summary>Fecha de actualización</summary>
    public string FechaActualizacion { get; set; } = string.Empty;

    /// <summary>Ciudad</summary>
    public string Ciudad { get; set; } = string.Empty;

    /// <summary>Facultad</summary>
    public int? Facultad { get; set; }
}

/// <summary>
/// La capa de datos del front para programa: la única pieza que sabe en qué
/// ruta vive este recurso. El transporte lo pone ClienteApi (D-v2-4).
/// </summary>
public class ServicioPrograma
{
    private readonly ClienteApi _api;

    public ServicioPrograma(ClienteApi api)
    {
        _api = api;
    }

    // RF1 — Listar
    public Task<Resultado<List<Programa>>> Listar(int limite = 1000) =>
        _api.Listar<Programa>($"/api/programa?limite={limite}");

    // RF2 — Obtener uno
    public Task<Resultado<Programa>> Obtener(int id) =>
        _api.Obtener<Programa>($"/api/programa/{id}");

    // RF3 — Crear: viaja la ficha entera, llave incluida.
    public Task<Resultado<bool>> Crear(Programa entidad) =>
        _api.Enviar(HttpMethod.Post, "/api/programa", entidad);

    // RF4 — Reemplazar: «guardar la ficha completa». La llave no va en el
    // cuerpo: identifica la fila y viaja en la ruta.
    public Task<Resultado<bool>> Reemplazar(int id, Programa entidad) =>
        _api.Enviar(HttpMethod.Put, $"/api/programa/{id}", new { nombre = entidad.Nombre, tipo = entidad.Tipo, nivel = entidad.Nivel, fechaCreacion = entidad.FechaCreacion, fechaCierre = entidad.FechaCierre, numeroCohortes = entidad.NumeroCohortes, cantGraduados = entidad.CantGraduados, fechaActualizacion = entidad.FechaActualizacion, ciudad = entidad.Ciudad, facultad = entidad.Facultad });

    // RF5 — Actualizar: «guardar solo lo que cambié». Viaja únicamente lo que
    // es distinto de lo que se cargó al abrir la edición (D-v2-10).
    public Task<Resultado<bool>> Actualizar(int id, Programa original, Programa editado)
    {
        var cuerpo = new Dictionary<string, object?>();
        if (!Equals(original.Nombre, editado.Nombre)) cuerpo["nombre"] = editado.Nombre;
        if (!Equals(original.Tipo, editado.Tipo)) cuerpo["tipo"] = editado.Tipo;
        if (!Equals(original.Nivel, editado.Nivel)) cuerpo["nivel"] = editado.Nivel;
        if (!Equals(original.FechaCreacion, editado.FechaCreacion)) cuerpo["fechaCreacion"] = editado.FechaCreacion;
        if (!Equals(original.FechaCierre, editado.FechaCierre)) cuerpo["fechaCierre"] = editado.FechaCierre;
        if (!Equals(original.NumeroCohortes, editado.NumeroCohortes)) cuerpo["numeroCohortes"] = editado.NumeroCohortes;
        if (!Equals(original.CantGraduados, editado.CantGraduados)) cuerpo["cantGraduados"] = editado.CantGraduados;
        if (!Equals(original.FechaActualizacion, editado.FechaActualizacion)) cuerpo["fechaActualizacion"] = editado.FechaActualizacion;
        if (!Equals(original.Ciudad, editado.Ciudad)) cuerpo["ciudad"] = editado.Ciudad;
        if (!Equals(original.Facultad, editado.Facultad)) cuerpo["facultad"] = editado.Facultad;

        if (cuerpo.Count == 0)
        {
            return Task.FromResult(Resultado<bool>.Mal(new() { "No cambió ningún dato." }));
        }

        return _api.Enviar(HttpMethod.Patch, $"/api/programa/{id}", cuerpo);
    }

    // RF6 — Retirar del uso (la API lo hace lógico: la fila no se borra)
    public Task<Resultado<bool>> Eliminar(int id) =>
        _api.Enviar(HttpMethod.Delete, $"/api/programa/{id}", null);
}
