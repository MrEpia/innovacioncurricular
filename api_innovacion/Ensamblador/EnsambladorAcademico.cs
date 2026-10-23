using ApiInnovacion.Repositorios;
using ApiInnovacion.Servicios;

namespace ApiInnovacion.Ensamblador;

/// <summary>
/// El ensamblador del bloque «Academico» de la v2: facultad, programa, acreditacion, registro_calificado.
///
/// Responsable: Sebastián Calvo. Este archivo lo llena SOLO su responsable, en su rama:
/// así los tres bloques de la v2 se integran sin tocarse (enmienda 1.1.0 del
/// Artículo 3).
/// </summary>
public static class EnsambladorAcademico
{
    public static IServiceCollection AgregarBloqueAcademico(this IServiceCollection servicios)
    {
        servicios.AddScoped<IRepositorioFacultad, RepositorioFacultadSqlServer>();
        servicios.AddScoped<IServicioFacultad, ServicioFacultad>();

        servicios.AddScoped<IRepositorioPrograma, RepositorioProgramaSqlServer>();
        servicios.AddScoped<IServicioPrograma, ServicioPrograma>();

        servicios.AddScoped<IRepositorioAcreditacion, RepositorioAcreditacionSqlServer>();
        servicios.AddScoped<IServicioAcreditacion, ServicioAcreditacion>();

        servicios.AddScoped<IRepositorioRegistroCalificado, RepositorioRegistroCalificadoSqlServer>();
        servicios.AddScoped<IServicioRegistroCalificado, ServicioRegistroCalificado>();

        return servicios;
    }
}
