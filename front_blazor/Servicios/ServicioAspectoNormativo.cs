using System.Net.Http.Json;
using System.Text.Json;

namespace FrontInnovacion.Servicios;

/// <summary>
/// AspectoNormativo, tal como el front lo maneja. Es una clase del front, no de la API:
/// se parece a la de allá porque el contrato es el mismo, pero son dos clases
/// distintas en dos proyectos distintos.
/// </summary>
public class AspectoNormativo
{
    public int Id { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string Fuente { get; set; } = string.Empty;
}

/// <summary>
/// La capa de datos del front para aspecto_normativo: la única pieza que sabe dónde
/// viven los datos —en la API, nunca en la base de datos— y la única que
/// habla HTTP para este recurso.
/// </summary>
public class ServicioAspectoNormativo
{
    private readonly HttpClient _http;

    private static readonly JsonSerializerOptions _opciones = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static readonly List<string> NoDisponible = new()
    {
        "El servicio no está disponible. ¿Está arriba la API?"
    };

    public ServicioAspectoNormativo(HttpClient http)
    {
        _http = http;
    }

    // RF1 — Listar
    public async Task<Resultado<List<AspectoNormativo>>> Listar(int limite = 1000)
    {
        try
        {
            var r = await _http.GetAsync($"/api/aspecto_normativo?limite={limite}");

            if (r.StatusCode == System.Net.HttpStatusCode.NoContent)
            {
                return Resultado<List<AspectoNormativo>>.Bien(new());
            }

            if (!r.IsSuccessStatusCode)
            {
                return Resultado<List<AspectoNormativo>>.Mal(await Mensajes(r));
            }

            var sobre = await r.Content.ReadFromJsonAsync<JsonElement>();
            var datos = sobre.GetProperty("datos")
                .Deserialize<List<AspectoNormativo>>(_opciones) ?? new();

            return Resultado<List<AspectoNormativo>>.Bien(datos);
        }
        catch (Exception e) when (e is HttpRequestException or TaskCanceledException)
        {
            return Resultado<List<AspectoNormativo>>.Mal(NoDisponible);
        }
    }

    // RF2 — Obtener uno
    public async Task<Resultado<AspectoNormativo>> Obtener(int id)
    {
        try
        {
            var r = await _http.GetAsync($"/api/aspecto_normativo/{id}");
            if (!r.IsSuccessStatusCode)
            {
                return Resultado<AspectoNormativo>.Mal(await Mensajes(r));
            }

            var ficha = await r.Content.ReadFromJsonAsync<AspectoNormativo>(_opciones);
            return Resultado<AspectoNormativo>.Bien(ficha!);
        }
        catch (Exception e) when (e is HttpRequestException or TaskCanceledException)
        {
            return Resultado<AspectoNormativo>.Mal(NoDisponible);
        }
    }

    // RF3 — Crear
    public async Task<Resultado<bool>> Crear(AspectoNormativo entidad)
    {
        return await Enviar(HttpMethod.Post, "/api/aspecto_normativo", entidad);
    }

    // RF4 — Reemplazar: «guardar la ficha completa». La llave no va en el
    // cuerpo: identifica la fila y viaja en la ruta.
    public async Task<Resultado<bool>> Reemplazar(int id, AspectoNormativo entidad)
    {
        var cuerpo = new
        {
            tipo = entidad.Tipo, descripcion = entidad.Descripcion, fuente = entidad.Fuente
        };
        return await Enviar(HttpMethod.Put, $"/api/aspecto_normativo/{id}", cuerpo);
    }

    // RF5 — Actualizar: «guardar solo lo que cambié». Solo viaja lo diligenciado.
    public async Task<Resultado<bool>> Actualizar(
        int id,
        string? tipo, string? descripcion, string? fuente)
    {
        var cuerpo = new Dictionary<string, object?>();
        if (!string.IsNullOrWhiteSpace(tipo)) cuerpo["tipo"] = tipo;
        if (!string.IsNullOrWhiteSpace(descripcion)) cuerpo["descripcion"] = descripcion;
        if (!string.IsNullOrWhiteSpace(fuente)) cuerpo["fuente"] = fuente;

        return await Enviar(HttpMethod.Patch, $"/api/aspecto_normativo/{id}", cuerpo);
    }

    // RF6 — Retirar del uso (la API lo hace lógico: la fila no se borra)
    public async Task<Resultado<bool>> Eliminar(int id)
    {
        return await Enviar(HttpMethod.Delete, $"/api/aspecto_normativo/{id}", null);
    }

    private async Task<Resultado<bool>> Enviar(HttpMethod metodo, string ruta, object? cuerpo)
    {
        try
        {
            var peticion = new HttpRequestMessage(metodo, ruta);
            if (cuerpo != null)
            {
                peticion.Content = JsonContent.Create(cuerpo);
            }

            var r = await _http.SendAsync(peticion);
            return r.IsSuccessStatusCode
                ? Resultado<bool>.Bien(true)
                : Resultado<bool>.Mal(await Mensajes(r));
        }
        catch (Exception e) when (e is HttpRequestException or TaskCanceledException)
        {
            return Resultado<bool>.Mal(NoDisponible);
        }
    }

    /// <summary>Traduce a texto los errores que produce esta API para aspecto_normativo.</summary>
    private static async Task<List<string>> Mensajes(HttpResponseMessage r)
    {
        try
        {
            var sobre = await r.Content.ReadFromJsonAsync<JsonElement>();

            if (sobre.TryGetProperty("errores", out var errores)
                && errores.ValueKind == JsonValueKind.Array
                && errores.GetArrayLength() > 0)
            {
                return errores.EnumerateArray()
                    .Select(x => x.GetString() ?? "")
                    .Where(x => x.Length > 0)
                    .ToList();
            }

            var partes = new List<string>();
            if (sobre.TryGetProperty("mensaje", out var m)) partes.Add(m.GetString() ?? "");
            if (sobre.TryGetProperty("detalle", out var dt)) partes.Add(dt.GetString() ?? "");
            partes.RemoveAll(string.IsNullOrWhiteSpace);

            return partes.Count > 0
                ? partes
                : new List<string> { "No se pudo completar la operación." };
        }
        catch
        {
            return new List<string> { "No se pudo completar la operación." };
        }
    }
}
