using ApiInnovacion.Ensamblador;
using Microsoft.AspNetCore.Mvc;
using ApiInnovacion.Infraestructura;
using ApiInnovacion.Repositorios.Postgres;
using Microsoft.Data.SqlClient;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

if (args.Contains("--migrar-postgres"))
{
    try { await MigradorPostgres.Ejecutar(builder.Configuration, args.Contains("--datos-demo")); }
    catch (PostgresException ex)
    {
        Console.Error.WriteLine($"Migración PostgreSQL fallida (SQLSTATE {ex.SqlState}). Revise la migración pendiente; no se publican datos ni credenciales.");
        Environment.ExitCode = 1;
    }
    catch (Exception ex)
    {
        // No imprimir excepciones del driver: pueden incluir datos privados.
        Console.Error.WriteLine(ex is InvalidOperationException ? ex.Message : $"Migración fallida ({ex.GetType().Name}). Revise acceso y esquema; no se publican credenciales.");
        Environment.ExitCode = 1;
    }
    return;
}

// La API escucha en el 8072 también DENTRO del contenedor, para que el Dockerfile,
// el docker-compose y los contratos digan todos el mismo número (3_plan.md §5.2).
var puerto = Environment.GetEnvironmentVariable("PORT") ?? "8072";
if (!int.TryParse(puerto, out var numeroPuerto) || numeroPuerto is < 1 or > 65535)
    throw new InvalidOperationException("PORT debe ser un puerto válido.");
builder.WebHost.UseUrls($"http://0.0.0.0:{puerto}");

// ============================================================
// EL ENSAMBLADOR (Artículo 3, enmienda 1.1.0)
// Cada bloque registra sus pares interfaz → clase concreta en su propio
// archivo de Ensamblador/. Aquí solo se llaman: Program.cs deja de ser el
// archivo que los tres estudiantes editan a la vez.
// ============================================================
builder.Services.AgregarTablasV1();
builder.Services.AgregarBloqueAcademico();      // v2 — Sebastián Calvo
builder.Services.AgregarBloqueActividades();    // v2 — David Epia
builder.Services.AgregarBloquePuentes();        // v2 — David Epia
builder.Services.AgregarSeguridad(builder.Configuration);   // v3 — JWT, roles, usuarios
builder.Services.AgregarMaestroDetalle();         // v3 — maestro-detalle (registro calificado y programa)
builder.Services.AgregarReportes();              // v4 — 10 consultas multitabla y dashboard
builder.Services.AgregarProveedorDatos(builder.Configuration);

builder.Services.AddControllers(o => o.Filters.Add<ErroresProduccion>());
var origenes = builder.Configuration.GetSection("Cors:Origenes").Get<string[]>() ?? [];
builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
{
    if (origenes.Length > 0) p.WithOrigins(origenes).AllowAnyHeader().AllowAnyMethod();
}));

// ============================================================
// EL 422 DEL CONTRATO (3_plan.md §4.9)
// Con [ApiController], un cuerpo inválido corta la petición ANTES de entrar al
// método y responde 400 con ProblemDetails. El contrato exige 422 con el sobre
// {estado, mensaje, errores[]}: hay que reemplazar la fábrica de respuestas.
// ============================================================
builder.Services.Configure<ApiBehaviorOptions>(opciones =>
{
    opciones.InvalidModelStateResponseFactory = contexto =>
    {
        // Los mensajes de las anotaciones ya están en español. Los que produce
        // el LECTOR DE JSON —un número donde va un texto, un JSON mal formado—
        // llegan en inglés y con jerga interna: se traducen aquí, señalando el
        // campo cuando se sabe cuál es (Artículo 8: todo en español).
        var errores = new List<string>();
        foreach (var (clave, estado) in contexto.ModelState)
        {
            foreach (var error in estado.Errors)
            {
                var deLector = error.Exception != null
                    || error.ErrorMessage.StartsWith("The JSON value", StringComparison.Ordinal)
                    || error.ErrorMessage.EndsWith("field is required.", StringComparison.Ordinal);

                if (!deLector)
                {
                    errores.Add(error.ErrorMessage);
                }
                else if (clave.StartsWith("$.", StringComparison.Ordinal))
                {
                    errores.Add($"El campo {clave[2..]} no tiene el formato esperado: revise el tipo del valor enviado.");
                }
                else
                {
                    errores.Add("El cuerpo de la petición no es un JSON válido.");
                }
            }
        }
        // Si ya se señaló el campo exacto, el aviso genérico del cuerpo sobra.
        if (errores.Any(e => e.StartsWith("El campo ", StringComparison.Ordinal)))
        {
            errores.Remove("El cuerpo de la petición no es un JSON válido.");
        }
        errores = errores.Distinct().ToList();

        return new ObjectResult(new
        {
            estado = 422,
            mensaje = "Datos inválidos.",
            errores
        })
        { StatusCode = 422 };
    };
});

builder.Services.AddEndpointsApiExplorer();
// v4: la API queda DOCUMENTADA en Swagger con los comentarios /// de cada
// controlador y cada modelo (el XML lo genera el compilador: ver el .csproj).
builder.Services.AddSwaggerGen(swagger =>
{
    swagger.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "API Innovación Curricular — Universidad Monte Verde",
        Version = "v4",
        Description = "CRUD de las 25 tablas del módulo, login con JWT, maestro-detalle, " +
                      "10 consultas multitabla (/api/reportes) y datos del dashboard (/api/dashboard). " +
                      "Para probar: POST /api/login, copie el token y use el botón Authorize."
    });
    var xml = Path.Combine(AppContext.BaseDirectory, "ApiInnovacion.xml");
    if (File.Exists(xml))
    {
        swagger.IncludeXmlComments(xml);
    }
});

var app = builder.Build();
app.UseCors();

app.MapGet("/health", async (IConfiguration configuracion, CancellationToken cancelacion) =>
{
    try
    {
        var proveedor = configuracion["BaseDatos:Proveedor"] ?? "SqlServer";
        await using System.Data.Common.DbConnection conexion = proveedor.Equals("Postgres", StringComparison.OrdinalIgnoreCase)
            ? new NpgsqlConnection(new ConfiguracionPostgres(configuracion).CadenaConexion)
            : new SqlConnection(configuracion.GetConnectionString("SqlServer"));
        await conexion.OpenAsync(cancelacion);
        await using var comando = conexion.CreateCommand();
        var esPostgres = proveedor.Equals("Postgres", StringComparison.OrdinalIgnoreCase);
        comando.CommandText = esPostgres
            ? "SELECT COUNT(*) FROM public.astra_migraciones WHERE version IN ('001_esquema.sql','002_operaciones_atomicas.sql','003_maestro_detalle.sql')"
            : "SELECT COUNT(*) FROM rol WHERE activo = 1";
        comando.CommandTimeout = 5;
        var comprobacion = await comando.ExecuteScalarAsync(cancelacion);
        if (esPostgres && Convert.ToInt64(comprobacion) != 3)
            return Results.Json(new { estado = "no disponible", baseDatos = "migraciones pendientes" }, statusCode: 503);
        return Results.Ok(new { estado = "saludable", baseDatos = "conectada" });
    }
    catch { return Results.Json(new { estado = "no disponible", baseDatos = "sin verificar" }, statusCode: 503); }
});

app.UseSwagger();
app.UseSwaggerUI();

// RF7 — Diagnóstico: dice quién es y qué versión, sin tocar la base de datos.
// Si esto no responde, el problema no es de contrato: la API no está arriba.
app.MapGet("/", () => Results.Ok(new
{
    mensaje = "API Innovación Curricular — v4: CRUD, JWT y roles, reportes multitabla y dashboard",
    version = "v4",
    contratos = "/swagger"
}));

// v3: autenticación y autorización ANTES de los controladores. El diagnóstico
// de arriba y Swagger siguen respondiendo sin token: sirven para saber si la
// API está viva y para conseguir el primero.
await app.UsarSeguridad();

app.MapControllers();

app.Run();
