namespace FrontInnovacion.Servicios;

/// <summary>
/// programa_ac, tal como el front lo maneja (v2 — bloque Puentes). Es una
/// clase del front, no de la API: se parece porque el contrato es el mismo.
///
/// Los números y las fechas son anulables para que un formulario a medio llenar
/// llegue a la API como «falta este dato» (422) y no como un cero inventado.
/// </summary>
public class ProgramaAc
{
    /// <summary>Programa</summary>
    public int? Programa { get; set; }

    /// <summary>Área de conocimiento</summary>
    public string AreaConocimiento { get; set; } = string.Empty;
}

/// <summary>
/// La capa de datos del front para programa_ac: la única pieza que sabe en qué
/// ruta vive este recurso. El transporte lo pone ClienteApi (D-v2-4).
/// </summary>
public class ServicioProgramaAc
{
    private readonly ClienteApi _api;

    public ServicioProgramaAc(ClienteApi api)
    {
        _api = api;
    }

    // RF1 — Listar
    public Task<Resultado<List<ProgramaAc>>> Listar(int limite = 1000) =>
        _api.Listar<ProgramaAc>($"/api/programa_ac?limite={limite}");

    // RF2 — Obtener uno
    public Task<Resultado<ProgramaAc>> Obtener(int programa, string areaConocimiento) =>
        _api.Obtener<ProgramaAc>($"/api/programa_ac/{programa}/{Uri.EscapeDataString(areaConocimiento)}");

    // RF3 — Crear: viaja la ficha entera, llave incluida.
    public Task<Resultado<bool>> Crear(ProgramaAc entidad) =>
        _api.Enviar(HttpMethod.Post, "/api/programa_ac", entidad);

    // RF6 — Retirar del uso (la API lo hace lógico: la fila no se borra)
    public Task<Resultado<bool>> Eliminar(int programa, string areaConocimiento) =>
        _api.Enviar(HttpMethod.Delete, $"/api/programa_ac/{programa}/{Uri.EscapeDataString(areaConocimiento)}", null);
}
