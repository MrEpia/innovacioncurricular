using FrontInnovacion.Servicios;

namespace FrontInnovacion.Ensamblador;

/// <summary>
/// Los servicios del front del bloque «Academico» de la v2: facultad, programa, acreditacion, registro_calificado.
/// Responsable: Sebastián Calvo. Solo su responsable edita este archivo.
///
/// Todos usan por dentro el ClienteApi que registra EnsambladorV1: aquí solo
/// se declara que cada recurso tiene su servicio (Artículo 10.1).
/// </summary>
public static class EnsambladorAcademico
{
    public static IServiceCollection AgregarServiciosAcademico(this IServiceCollection servicios)
    {
        servicios.AddScoped<ServicioFacultad>();
        servicios.AddScoped<ServicioPrograma>();
        servicios.AddScoped<ServicioAcreditacion>();
        servicios.AddScoped<ServicioRegistroCalificado>();

        return servicios;
    }
}
