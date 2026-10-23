namespace FrontInnovacion.Servicios;

/// <summary>
/// programa_pe, tal como el front lo maneja (v2 — bloque Puentes). Es una
/// clase del front, no de la API: se parece porque el contrato es el mismo.
///
/// Los números y las fechas son anulables para que un formulario a medio llenar
/// llegue a la API como «falta este dato» (422) y no como un cero inventado.
/// </summary>
public class ProgramaPe
{
    /// <summary>Programa</summary>
    public int? Programa { get; set; }

    /// <summary>Práctica o estrategia</summary>
    public int? PracticaEstrategia { get; set; }
}

/// <summary>
/// La capa de datos del front para programa_pe: la única pieza que sabe en qué
/// ruta vive este recurso. El transporte lo pone ClienteApi (D-v2-4).
/// </summary>
public class ServicioProgramaPe
{
    private readonly ClienteApi _api;

    public ServicioProgramaPe(ClienteApi api)
    {
        _api = api;
    }

    // RF1 — Listar
    public Task<Resultado<List<ProgramaPe>>> Listar(int limite = 1000) =>
        _api.Listar<ProgramaPe>($"/api/programa_pe?limite={limite}");

    // RF2 — Obtener uno
    public Task<Resultado<ProgramaPe>> Obtener(int programa, int practicaEstrategia) =>
        _api.Obtener<ProgramaPe>($"/api/programa_pe/{programa}/{practicaEstrategia}");

    // RF3 — Crear: viaja la ficha entera, llave incluida.
    public Task<Resultado<bool>> Crear(ProgramaPe entidad) =>
        _api.Enviar(HttpMethod.Post, "/api/programa_pe", entidad);

    // RF6 — Retirar del uso (la API lo hace lógico: la fila no se borra)
    public Task<Resultado<bool>> Eliminar(int programa, int practicaEstrategia) =>
        _api.Enviar(HttpMethod.Delete, $"/api/programa_pe/{programa}/{practicaEstrategia}", null);
}
