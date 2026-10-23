namespace FrontInnovacion.Servicios;

/// <summary>
/// Aliado, tal como el front lo maneja.
///
/// **Es una clase del front, no de la API.** Se parece a la de allá porque el
/// contrato es el mismo, y aun así son dos clases distintas en dos proyectos
/// distintos: si compartieran una biblioteca, los dos procesos dejarían de ser
/// independientes y el front podría romperse por un cambio interno de la API.
///
/// Lo único que los une es el JSON.
/// </summary>
public class Aliado
{
    /// <summary>NIT</summary>
    public int Nit { get; set; }

    /// <summary>Razón social</summary>
    public string RazonSocial { get; set; } = string.Empty;

    /// <summary>Nombre del contacto</summary>
    public string NombreContacto { get; set; } = string.Empty;

    /// <summary>Correo</summary>
    public string Correo { get; set; } = string.Empty;

    /// <summary>Teléfono</summary>
    public string Telefono { get; set; } = string.Empty;

    /// <summary>Ciudad</summary>
    public string Ciudad { get; set; } = string.Empty;
}

/// <summary>
/// ==========================================================================
/// LA CAPA DE DATOS DEL FRONT — y por qué es de `aliado` y no «de cualquier
/// tabla»
/// ==========================================================================
///
/// Este servicio es al front lo que el repositorio es a la API: la ÚNICA pieza
/// que sabe dónde viven los datos —en la API, nunca en la base de datos— y la
/// única que habla HTTP.
///
/// **Y es específico de un recurso, no genérico.** Podría escribirse un
/// `ApiService.Listar("aliado")` que sirviera para cualquier tabla, y sería
/// más corto. No se hace, por el Artículo 10.1 y por lo mismo que del lado de
/// la API: un método `Listar(string tabla)` no le dice a nadie qué recursos
/// existen, y el compilador deja de revisar si esa tabla es una de las que hay.
///
/// Cuando el proyecto tenga más recursos habrá un servicio como este por cada
/// uno. Se van a parecer mucho — y cada uno va a decir sus campos, sus
/// mensajes y sus operaciones, que es justamente lo que un molde único borra.
///
/// ==========================================================================
/// LO QUE ESTE ARCHIVO NO SABE, Y NO LE HACE FALTA
/// ==========================================================================
///
/// No sabe que la API está en C#. Da la casualidad de que sí lo está —el front
/// también— pero en ninguna línea se aprovecha: todo viaja como JSON por HTTP,
/// igual que si la API estuviera en Python.
///
/// Y no sabe que detrás hay SQL Server. Eso es asunto de la API.
/// </summary>
public class ServicioAliado
{
    // El transporte HTTP (sobres, 204, errores, API caída) vive en ClienteApi
    // desde la v2. Este servicio sigue siendo el que sabe QUÉ recurso es, qué
    // ruta lleva y qué campos tiene.
    private readonly ClienteApi _api;

    public ServicioAliado(ClienteApi api)
    {
        _api = api;
    }

    // RF1 — Listar
    public Task<Resultado<List<Aliado>>> Listar(int limite = 1000) =>
        _api.Listar<Aliado>($"/api/aliado?limite={limite}");

    // RF2 — Obtener uno
    public Task<Resultado<Aliado>> Obtener(int nit) =>
        _api.Obtener<Aliado>($"/api/aliado/{nit}");

    // RF3 — Crear
    public Task<Resultado<bool>> Crear(Aliado entidad) =>
        _api.Enviar(HttpMethod.Post, "/api/aliado", entidad);

    // RF4 — Reemplazar: «guardar la ficha completa». La llave no va en el
    // cuerpo: identifica la fila y viaja en la ruta.
    public Task<Resultado<bool>> Reemplazar(int nit, Aliado entidad) =>
        _api.Enviar(HttpMethod.Put, $"/api/aliado/{nit}", new { razonSocial = entidad.RazonSocial, nombreContacto = entidad.NombreContacto, correo = entidad.Correo, telefono = entidad.Telefono, ciudad = entidad.Ciudad });

    // RF5 — Actualizar: «guardar solo lo que cambié». Solo viaja lo diligenciado.
    public Task<Resultado<bool>> Actualizar(int nit, string? razonSocial, string? nombreContacto, string? correo, string? telefono, string? ciudad)
    {
        var cuerpo = new Dictionary<string, object?>();
        if (!string.IsNullOrWhiteSpace(razonSocial)) cuerpo["razonSocial"] = razonSocial;
        if (!string.IsNullOrWhiteSpace(nombreContacto)) cuerpo["nombreContacto"] = nombreContacto;
        if (!string.IsNullOrWhiteSpace(correo)) cuerpo["correo"] = correo;
        if (!string.IsNullOrWhiteSpace(telefono)) cuerpo["telefono"] = telefono;
        if (!string.IsNullOrWhiteSpace(ciudad)) cuerpo["ciudad"] = ciudad;

        return _api.Enviar(HttpMethod.Patch, $"/api/aliado/{nit}", cuerpo);
    }

    // RF6 — Retirar del uso (la API lo hace lógico: la fila no se borra)
    public Task<Resultado<bool>> Eliminar(int nit) =>
        _api.Enviar(HttpMethod.Delete, $"/api/aliado/{nit}", null);
}
