using ApiInnovacion.Repositorios;
using ApiInnovacion.Servicios;

namespace ApiInnovacion.Ensamblador;

/// <summary>
/// El ENSAMBLADOR de la v1: las siete tablas sin clave foránea.
///
/// Antes estas líneas vivían sueltas en Program.cs. Se movieron aquí, sin
/// cambiar una sola, por la enmienda 1.1.0 del Artículo 3: con tres
/// estudiantes agregando tablas al mismo tiempo, un Program.cs compartido es
/// un conflicto de fusión en cada ronda. Ahora cada bloque tiene su archivo y
/// Program.cs solo los llama.
///
/// Sigue siendo el ÚNICO lugar donde una clase concreta aparece junto a su
/// interfaz: el ensamblador se repartió en archivos, no en responsabilidades.
/// </summary>
public static class EnsambladorV1
{
    public static IServiceCollection AgregarTablasV1(this IServiceCollection servicios)
    {
        servicios.AddScoped<IRepositorioAliado, RepositorioAliadoSqlServer>();
        servicios.AddScoped<IServicioAliado, ServicioAliado>();

        servicios.AddScoped<IRepositorioAreaConocimiento, RepositorioAreaConocimientoSqlServer>();
        servicios.AddScoped<IServicioAreaConocimiento, ServicioAreaConocimiento>();

        servicios.AddScoped<IRepositorioUniversidad, RepositorioUniversidadSqlServer>();
        servicios.AddScoped<IServicioUniversidad, ServicioUniversidad>();

        servicios.AddScoped<IRepositorioAspectoNormativo, RepositorioAspectoNormativoSqlServer>();
        servicios.AddScoped<IServicioAspectoNormativo, ServicioAspectoNormativo>();

        servicios.AddScoped<IRepositorioPracticaEstrategia, RepositorioPracticaEstrategiaSqlServer>();
        servicios.AddScoped<IServicioPracticaEstrategia, ServicioPracticaEstrategia>();

        servicios.AddScoped<IRepositorioEnfoque, RepositorioEnfoqueSqlServer>();
        servicios.AddScoped<IServicioEnfoque, ServicioEnfoque>();

        servicios.AddScoped<IRepositorioCarInnovacion, RepositorioCarInnovacionSqlServer>();
        servicios.AddScoped<IServicioCarInnovacion, ServicioCarInnovacion>();

        // Compartido por las 22 tablas: ¿existe y está activo el registro al que
        // apunta una clave foránea? ¿tiene hijos activos el que se quiere retirar?
        servicios.AddScoped<IRepositorioReferencias, RepositorioReferenciasSqlServer>();

        return servicios;
    }
}
