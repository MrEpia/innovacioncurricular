using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Components;

namespace FrontInnovacion.Servicios;

/// <summary>
/// ==========================================================================
/// EL TRANSPORTE HACIA LA API — y por qué NO es un «servicio genérico»
/// ==========================================================================
///
/// En la v1 cada uno de los siete servicios traía su propia copia de lo mismo:
/// enviar la petición, distinguir el 204, leer el sobre {datos}, traducir el
/// sobre de error a mensajes y avisar si la API no responde. Con veintidós
/// recursos, eran veintidós copias de las mismas sesenta líneas, y el día de
/// la v3 —cuando cada petición tenga que llevar el token— habría que cambiarlas
/// todas. Por eso la v2 las junta aquí (decisión D-v2-4).
///
/// Lo que esta clase NO es: un `ApiService.Listar("aliado")`. Ninguna página
/// la usa. Las páginas siguen llamando a ServicioAliado, ServicioFacultad…, que
/// son los que saben qué recurso existe, qué campos tiene y qué ruta lleva
/// (Artículo 10.1). Esta clase solo sabe HABLAR HTTP: es al front lo que
/// HttpClient es a cualquier programa.
///
/// Sigue sin saber que detrás hay SQL Server, ni siquiera que la API está en C#.
///
/// v3 (David Epia): este es el ÚNICO lugar que tuvo que cambiar para que los
/// 22 servicios manden el token. Cada petición sale con
/// Authorization: Bearer &lt;token de la sesión&gt;; si la API responde 401 (el
/// token venció), se cierra la sesión y se lleva al usuario a ingresar; si
/// responde 403, se le dice que su rol no lo permite.
/// </summary>
public class ClienteApi
{
    private readonly HttpClient _http;
    private readonly ProveedorToken _token;
    private readonly NavigationManager _navegacion;

    private static readonly JsonSerializerOptions _opciones = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static readonly List<string> NoDisponible = new()
    {
        "El servicio no está disponible. ¿Está arriba la API?"
    };

    private static readonly List<string> SinPermiso = new()
    {
        "Su rol no permite esta operación."
    };

    public ClienteApi(HttpClient http, ProveedorToken token, NavigationManager navegacion)
    {
        _http = http;
        _token = token;
        _navegacion = navegacion;
    }

    /// <summary>Envía la petición con el token de la sesión.</summary>
    private async Task<HttpResponseMessage> EnviarConToken(HttpRequestMessage peticion)
    {
        var token = await _token.Obtener();
        if (!string.IsNullOrEmpty(token))
        {
            peticion.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        var r = await _http.SendAsync(peticion);

        if (r.StatusCode == HttpStatusCode.Unauthorized)
        {
            // El token venció (o la API ya no lo reconoce): se cierra la sesión
            // del servidor y se pide ingresar de nuevo, con el aviso.
            _navegacion.NavigateTo("/sesion/expirada", forceLoad: true);
        }

        return r;
    }

    /// <summary>
    /// GET de un listado. El 204 es «no hay ninguno» y NO es un error: la
    /// pantalla muestra un recuadro que lo dice, no un aviso rojo.
    /// </summary>
    public async Task<Resultado<List<T>>> Listar<T>(string ruta)
    {
        try
        {
            var r = await EnviarConToken(new HttpRequestMessage(HttpMethod.Get, ruta));

            if (r.StatusCode == HttpStatusCode.NoContent)
            {
                return Resultado<List<T>>.Bien(new());
            }

            if (!r.IsSuccessStatusCode)
            {
                return Resultado<List<T>>.Mal(await Mensajes(r));
            }

            // El sobre del contrato: { tabla, limite, total, datos[] }
            var sobre = await r.Content.ReadFromJsonAsync<JsonElement>();
            var datos = sobre.GetProperty("datos").Deserialize<List<T>>(_opciones) ?? new();
            return Resultado<List<T>>.Bien(datos);
        }
        catch (Exception e) when (e is HttpRequestException or TaskCanceledException)
        {
            return Resultado<List<T>>.Mal(NoDisponible);
        }
    }

    /// <summary>GET de un registro por su llave.</summary>
    public async Task<Resultado<T>> Obtener<T>(string ruta)
    {
        try
        {
            var r = await EnviarConToken(new HttpRequestMessage(HttpMethod.Get, ruta));
            if (!r.IsSuccessStatusCode)
            {
                return Resultado<T>.Mal(await Mensajes(r));
            }

            var ficha = await r.Content.ReadFromJsonAsync<T>(_opciones);
            return Resultado<T>.Bien(ficha!);
        }
        catch (Exception e) when (e is HttpRequestException or TaskCanceledException)
        {
            return Resultado<T>.Mal(NoDisponible);
        }
    }

    /// <summary>POST, PUT, PATCH o DELETE: ¿salió bien, y si no, por qué?</summary>
    public async Task<Resultado<bool>> Enviar(HttpMethod metodo, string ruta, object? cuerpo)
    {
        try
        {
            var peticion = new HttpRequestMessage(metodo, ruta);
            if (cuerpo != null)
            {
                peticion.Content = JsonContent.Create(cuerpo);
            }

            var r = await EnviarConToken(peticion);
            return r.IsSuccessStatusCode
                ? Resultado<bool>.Bien(true)
                : Resultado<bool>.Mal(await Mensajes(r));
        }
        catch (Exception e) when (e is HttpRequestException or TaskCanceledException)
        {
            return Resultado<bool>.Mal(NoDisponible);
        }
    }

    /// <summary>
    /// Traduce a texto el sobre de error del contrato: la lista `errores` del
    /// 422 si viene, y si no, `mensaje` + `detalle`.
    /// </summary>
    private static async Task<List<string>> Mensajes(HttpResponseMessage r)
    {
        if (r.StatusCode == HttpStatusCode.Forbidden)
        {
            return SinPermiso;
        }

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
            if (sobre.TryGetProperty("detalle", out var d)) partes.Add(d.GetString() ?? "");
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
