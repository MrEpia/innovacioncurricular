namespace FrontInnovacion.Servicios;

/// <summary>
/// enfoque_rc, tal como el front lo maneja (v2 — bloque Puentes). Es una
/// clase del front, no de la API: se parece porque el contrato es el mismo.
///
/// Los números y las fechas son anulables para que un formulario a medio llenar
/// llegue a la API como «falta este dato» (422) y no como un cero inventado.
/// </summary>
public class EnfoqueRc
{
    /// <summary>Enfoque</summary>
    public int? Enfoque { get; set; }

    /// <summary>Registro calificado</summary>
    public int? RegistroCalificado { get; set; }
}

/// <summary>
/// La capa de datos del front para enfoque_rc: la única pieza que sabe en qué
/// ruta vive este recurso. El transporte lo pone ClienteApi (D-v2-4).
/// </summary>
public class ServicioEnfoqueRc
{
    private readonly ClienteApi _api;

    public ServicioEnfoqueRc(ClienteApi api)
    {
        _api = api;
    }

    // RF1 — Listar
    public Task<Resultado<List<EnfoqueRc>>> Listar(int limite = 1000) =>
        _api.Listar<EnfoqueRc>($"/api/enfoque_rc?limite={limite}");

    // RF2 — Obtener uno
    public Task<Resultado<EnfoqueRc>> Obtener(int enfoque, int registroCalificado) =>
        _api.Obtener<EnfoqueRc>($"/api/enfoque_rc/{enfoque}/{registroCalificado}");

    // RF3 — Crear: viaja la ficha entera, llave incluida.
    public Task<Resultado<bool>> Crear(EnfoqueRc entidad) =>
        _api.Enviar(HttpMethod.Post, "/api/enfoque_rc", entidad);

    // RF6 — Retirar del uso (la API lo hace lógico: la fila no se borra)
    public Task<Resultado<bool>> Eliminar(int enfoque, int registroCalificado) =>
        _api.Enviar(HttpMethod.Delete, $"/api/enfoque_rc/{enfoque}/{registroCalificado}", null);
}
