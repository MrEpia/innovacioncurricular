namespace FrontInnovacion.Servicios;

/// <summary>
/// programa_ci, tal como el front lo maneja (v2 — bloque Puentes). Es una
/// clase del front, no de la API: se parece porque el contrato es el mismo.
///
/// Los números y las fechas son anulables para que un formulario a medio llenar
/// llegue a la API como «falta este dato» (422) y no como un cero inventado.
/// </summary>
public class ProgramaCi
{
    /// <summary>Programa</summary>
    public int? Programa { get; set; }

    /// <summary>Característica de innovación</summary>
    public int? CarInnovacion { get; set; }
}

/// <summary>
/// La capa de datos del front para programa_ci: la única pieza que sabe en qué
/// ruta vive este recurso. El transporte lo pone ClienteApi (D-v2-4).
/// </summary>
public class ServicioProgramaCi
{
    private readonly ClienteApi _api;

    public ServicioProgramaCi(ClienteApi api)
    {
        _api = api;
    }

    // RF1 — Listar
    public Task<Resultado<List<ProgramaCi>>> Listar(int limite = 1000) =>
        _api.Listar<ProgramaCi>($"/api/programa_ci?limite={limite}");

    // RF2 — Obtener uno
    public Task<Resultado<ProgramaCi>> Obtener(int programa, int carInnovacion) =>
        _api.Obtener<ProgramaCi>($"/api/programa_ci/{programa}/{carInnovacion}");

    // RF3 — Crear: viaja la ficha entera, llave incluida.
    public Task<Resultado<bool>> Crear(ProgramaCi entidad) =>
        _api.Enviar(HttpMethod.Post, "/api/programa_ci", entidad);

    // RF6 — Retirar del uso (la API lo hace lógico: la fila no se borra)
    public Task<Resultado<bool>> Eliminar(int programa, int carInnovacion) =>
        _api.Enviar(HttpMethod.Delete, $"/api/programa_ci/{programa}/{carInnovacion}", null);
}
