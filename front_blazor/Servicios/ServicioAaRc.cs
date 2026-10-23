namespace FrontInnovacion.Servicios;

/// <summary>
/// aa_rc, tal como el front lo maneja (v2 — bloque Actividades). Es una
/// clase del front, no de la API: se parece porque el contrato es el mismo.
///
/// Los números y las fechas son anulables para que un formulario a medio llenar
/// llegue a la API como «falta este dato» (422) y no como un cero inventado.
/// </summary>
public class AaRc
{
    /// <summary>Actividad académica</summary>
    public int? ActivAcademicasIdcurso { get; set; }

    /// <summary>Registro calificado</summary>
    public int? RegistroCalificadoCodigo { get; set; }

    /// <summary>Componente</summary>
    public string Componente { get; set; } = string.Empty;

    /// <summary>Semestre</summary>
    public string Semestre { get; set; } = string.Empty;
}

/// <summary>
/// La capa de datos del front para aa_rc: la única pieza que sabe en qué
/// ruta vive este recurso. El transporte lo pone ClienteApi (D-v2-4).
/// </summary>
public class ServicioAaRc
{
    private readonly ClienteApi _api;

    public ServicioAaRc(ClienteApi api)
    {
        _api = api;
    }

    // RF1 — Listar
    public Task<Resultado<List<AaRc>>> Listar(int limite = 1000) =>
        _api.Listar<AaRc>($"/api/aa_rc?limite={limite}");

    // RF2 — Obtener uno
    public Task<Resultado<AaRc>> Obtener(int activAcademicasIdcurso, int registroCalificadoCodigo) =>
        _api.Obtener<AaRc>($"/api/aa_rc/{activAcademicasIdcurso}/{registroCalificadoCodigo}");

    // RF3 — Crear: viaja la ficha entera, llave incluida.
    public Task<Resultado<bool>> Crear(AaRc entidad) =>
        _api.Enviar(HttpMethod.Post, "/api/aa_rc", entidad);

    // RF4 — Reemplazar: «guardar la ficha completa». La llave no va en el
    // cuerpo: identifica la fila y viaja en la ruta.
    public Task<Resultado<bool>> Reemplazar(int activAcademicasIdcurso, int registroCalificadoCodigo, AaRc entidad) =>
        _api.Enviar(HttpMethod.Put, $"/api/aa_rc/{activAcademicasIdcurso}/{registroCalificadoCodigo}", new { componente = entidad.Componente, semestre = entidad.Semestre });

    // RF5 — Actualizar: «guardar solo lo que cambié». Viaja únicamente lo que
    // es distinto de lo que se cargó al abrir la edición (D-v2-10).
    public Task<Resultado<bool>> Actualizar(int activAcademicasIdcurso, int registroCalificadoCodigo, AaRc original, AaRc editado)
    {
        var cuerpo = new Dictionary<string, object?>();
        if (!Equals(original.Componente, editado.Componente)) cuerpo["componente"] = editado.Componente;
        if (!Equals(original.Semestre, editado.Semestre)) cuerpo["semestre"] = editado.Semestre;

        if (cuerpo.Count == 0)
        {
            return Task.FromResult(Resultado<bool>.Mal(new() { "No cambió ningún dato." }));
        }

        return _api.Enviar(HttpMethod.Patch, $"/api/aa_rc/{activAcademicasIdcurso}/{registroCalificadoCodigo}", cuerpo);
    }

    // RF6 — Retirar del uso (la API lo hace lógico: la fila no se borra)
    public Task<Resultado<bool>> Eliminar(int activAcademicasIdcurso, int registroCalificadoCodigo) =>
        _api.Enviar(HttpMethod.Delete, $"/api/aa_rc/{activAcademicasIdcurso}/{registroCalificadoCodigo}", null);
}
