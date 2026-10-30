using ApiInnovacion.Ensamblador;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// La API escucha en el 8072 también DENTRO del contenedor, para que el Dockerfile,
// el docker-compose y los contratos digan todos el mismo número (3_plan.md §5.2).
builder.WebHost.UseUrls("http://0.0.0.0:8072");

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

builder.Services.AddControllers();

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
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

// RF7 — Diagnóstico: dice quién es y qué versión, sin tocar la base de datos.
// Si esto no responde, el problema no es de contrato: la API no está arriba.
app.MapGet("/", () => Results.Ok(new
{
    mensaje = "API Innovación Curricular — v3: las 22 tablas con JWT y roles",
    version = "v3",
    contratos = "/swagger"
}));

// v3: autenticación y autorización ANTES de los controladores. El diagnóstico
// de arriba y Swagger siguen respondiendo sin token: sirven para saber si la
// API está viva y para conseguir el primero.
await app.UsarSeguridad();

app.MapControllers();

app.Run();
