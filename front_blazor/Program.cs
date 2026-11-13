using FrontInnovacion.Components;
using FrontInnovacion.Ensamblador;

// v4: números y fechas como se escriben en Colombia (14.472, no 14,472).
var cultura = new System.Globalization.CultureInfo("es-CO");
System.Globalization.CultureInfo.DefaultThreadCurrentCulture = cultura;
System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = cultura;

var builder = WebApplication.CreateBuilder(args);
var puertoRender = Environment.GetEnvironmentVariable("PORT");
if (puertoRender != null)
{
    if (!int.TryParse(puertoRender, out var numeroPuerto) || numeroPuerto is < 1 or > 65535)
        throw new InvalidOperationException("PORT debe ser un puerto válido.");
    builder.WebHost.UseUrls($"http://0.0.0.0:{puertoRender}");
}

// Blazor Server: el componente se renderiza en el servidor y el navegador
// recibe el HTML ya armado, manteniendo una conexión para los eventos.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// ============================================================
// DE DÓNDE SALEN LOS DATOS: de la API, por HTTP (Ensamblador/).
// ============================================================
var urlApi = builder.Configuration["UrlApi"] ?? "http://localhost:8072";
if (Environment.GetEnvironmentVariable("RENDER") == "true" &&
    (!Uri.TryCreate(urlApi, UriKind.Absolute, out var direccionApi) || direccionApi.IsLoopback || direccionApi.Scheme != "https"))
    throw new InvalidOperationException("Render requiere UrlApi HTTPS pública explícita.");

// Fuera de Docker vale lo de appsettings.json; dentro, el compose la
// sobreescribe con el NOMBRE del servicio —`api-innovacion`—, porque
// `localhost` dentro de un contenedor es el contenedor mismo.
builder.Services.AgregarServiciosV1(urlApi);
builder.Services.AgregarServiciosAcademico();     // v2 — Sebastián Calvo
builder.Services.AgregarServiciosActividades();   // v2 — David Epia
builder.Services.AgregarServiciosPuentes();       // v2 — David Epia
builder.Services.AgregarSesion(urlApi);           // v3 — David Epia: ingreso y sesión
builder.Services.AgregarAdministracion();         // v3 — David Epia: usuarios y roles
builder.Services.AgregarMaestroDetalle();          // v3 — maestro-detalle
builder.Services.AgregarReportes();               // v4 — reportes y dashboard

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

// v3: quién es (cookie de sesión) y qué puede, ANTES de la antifalsificación y
// de las pantallas.
app.UsarSesion();
app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.MapearSesion();                               // /sesion/salir y /sesion/expirada
app.MapGet("/health", () => Results.Ok(new { estado = "saludable", componente = "front" }));

app.Run();
