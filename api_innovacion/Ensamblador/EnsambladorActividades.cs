using ApiInnovacion.Repositorios;
using ApiInnovacion.Servicios;

namespace ApiInnovacion.Ensamblador;

/// <summary>
/// El ensamblador del bloque «Actividades» de la v2: activ_academica, pasantia, premio, docente_departamento, alianza, aa_rc.
///
/// Responsable: David Epia. Este archivo lo llena SOLO su responsable, en su rama:
/// así los tres bloques de la v2 se integran sin tocarse (enmienda 1.1.0 del
/// Artículo 3).
/// </summary>
public static class EnsambladorActividades
{
    public static IServiceCollection AgregarBloqueActividades(this IServiceCollection servicios)
    {
        servicios.AddScoped<IRepositorioActivAcademica, RepositorioActivAcademicaSqlServer>();
        servicios.AddScoped<IServicioActivAcademica, ServicioActivAcademica>();

        servicios.AddScoped<IRepositorioPasantia, RepositorioPasantiaSqlServer>();
        servicios.AddScoped<IServicioPasantia, ServicioPasantia>();

        servicios.AddScoped<IRepositorioPremio, RepositorioPremioSqlServer>();
        servicios.AddScoped<IServicioPremio, ServicioPremio>();

        servicios.AddScoped<IRepositorioDocenteDepartamento, RepositorioDocenteDepartamentoSqlServer>();
        servicios.AddScoped<IServicioDocenteDepartamento, ServicioDocenteDepartamento>();

        servicios.AddScoped<IRepositorioAlianza, RepositorioAlianzaSqlServer>();
        servicios.AddScoped<IServicioAlianza, ServicioAlianza>();

        servicios.AddScoped<IRepositorioAaRc, RepositorioAaRcSqlServer>();
        servicios.AddScoped<IServicioAaRc, ServicioAaRc>();

        return servicios;
    }
}
