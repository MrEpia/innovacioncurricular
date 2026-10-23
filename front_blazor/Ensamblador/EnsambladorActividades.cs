using FrontInnovacion.Servicios;

namespace FrontInnovacion.Ensamblador;

/// <summary>
/// Los servicios del front del bloque «Actividades» de la v2: activ_academica, pasantia, premio, docente_departamento, alianza, aa_rc.
/// Responsable: David Epia. Solo su responsable edita este archivo.
///
/// Todos usan por dentro el ClienteApi que registra EnsambladorV1: aquí solo
/// se declara que cada recurso tiene su servicio (Artículo 10.1).
/// </summary>
public static class EnsambladorActividades
{
    public static IServiceCollection AgregarServiciosActividades(this IServiceCollection servicios)
    {
        servicios.AddScoped<ServicioActivAcademica>();
        servicios.AddScoped<ServicioPasantia>();
        servicios.AddScoped<ServicioPremio>();
        servicios.AddScoped<ServicioDocenteDepartamento>();
        servicios.AddScoped<ServicioAlianza>();
        servicios.AddScoped<ServicioAaRc>();

        return servicios;
    }
}
