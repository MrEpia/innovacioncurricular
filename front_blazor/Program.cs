using FrontInnovacion.Components;
using FrontInnovacion.Ensamblador;

var builder = WebApplication.CreateBuilder(args);

// Blazor Server: el componente se renderiza en el servidor y el navegador
// recibe el HTML ya armado, manteniendo una conexión para los eventos.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// ============================================================
// DE DÓNDE SALEN LOS DATOS: de la API, por HTTP (Ensamblador/).
// ============================================================
var urlApi = builder.Configuration["UrlApi"] ?? "http://localhost:8072";

// Fuera de Docker vale lo de appsettings.json; dentro, el compose la
// sobreescribe con el NOMBRE del servicio —`api-innovacion`—, porque
// `localhost` dentro de un contenedor es el contenedor mismo.
builder.Services.AgregarServiciosV1(urlApi);
builder.Services.AgregarServiciosAcademico();     // v2 — Sebastián Calvo
builder.Services.AgregarServiciosActividades();   // v2 — David Epia
builder.Services.AgregarServiciosPuentes();       // v2 — Sebastián Brunal
builder.Services.AgregarSesion(urlApi);           // v3 — David Epia: ingreso y sesión
builder.Services.AgregarAdministracion();         // v3 — Sebastián Brunal: usuarios y roles

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

app.Run();
