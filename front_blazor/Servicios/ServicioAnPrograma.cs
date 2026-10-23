namespace FrontInnovacion.Servicios;

/// <summary>
/// an_programa, tal como el front lo maneja (v2 — bloque Puentes). Es una
/// clase del front, no de la API: se parece porque el contrato es el mismo.
///
/// Los números y las fechas son anulables para que un formulario a medio llenar
/// llegue a la API como «falta este dato» (422) y no como un cero inventado.
/// </summary>
public class AnPrograma
{
    /// <summary>Aspecto normativo</summary>
    public int? AspectoNormativo { get; set; }

    /// <summary>Programa</summary>
    public int? Programa { get; set; }
}

/// <summary>
/// La capa de datos del front para an_programa: la única pieza que sabe en qué
/// ruta vive este recurso. El transporte lo pone ClienteApi (D-v2-4).
/// </summary>
public class ServicioAnPrograma
{
    private readonly ClienteApi _api;

    public ServicioAnPrograma(ClienteApi api)
    {
        _api = api;
    }

    // RF1 — Listar
    public Task<Resultado<List<AnPrograma>>> Listar(int limite = 1000) =>
        _api.Listar<AnPrograma>($"/api/an_programa?limite={limite}");

    // RF2 — Obtener uno
    public Task<Resultado<AnPrograma>> Obtener(int aspectoNormativo, int programa) =>
        _api.Obtener<AnPrograma>($"/api/an_programa/{aspectoNormativo}/{programa}");

    // RF3 — Crear: viaja la ficha entera, llave incluida.
    public Task<Resultado<bool>> Crear(AnPrograma entidad) =>
        _api.Enviar(HttpMethod.Post, "/api/an_programa", entidad);

    // RF6 — Retirar del uso (la API lo hace lógico: la fila no se borra)
    public Task<Resultado<bool>> Eliminar(int aspectoNormativo, int programa) =>
        _api.Enviar(HttpMethod.Delete, $"/api/an_programa/{aspectoNormativo}/{programa}", null);
}
