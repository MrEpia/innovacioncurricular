using ApiInnovacion.Repositorios;
using ApiInnovacion.Repositorios.Postgres;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ApiInnovacion.Ensamblador;

public static class EnsambladorPostgres
{
    // Se llama después de los ensambladores históricos: SqlServer permanece por defecto.
    public static void AgregarProveedorDatos(this IServiceCollection servicios, IConfiguration configuracion)
    {
        var proveedor = configuracion["BaseDatos:Proveedor"] ?? "SqlServer";
        if (proveedor.Equals("SqlServer", StringComparison.OrdinalIgnoreCase)) return;
        if (!proveedor.Equals("Postgres", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("BaseDatos__Proveedor debe ser SqlServer o Postgres.");
        servicios.AddSingleton(new ConfiguracionPostgres(configuracion));
        servicios.Replace(ServiceDescriptor.Scoped<IRepositorioAaRc, RepositorioAaRcPostgres>());
        servicios.Replace(ServiceDescriptor.Scoped<IRepositorioAcreditacion, RepositorioAcreditacionPostgres>());
        servicios.Replace(ServiceDescriptor.Scoped<IRepositorioActivAcademica, RepositorioActivAcademicaPostgres>());
        servicios.Replace(ServiceDescriptor.Scoped<IRepositorioAliado, RepositorioAliadoPostgres>());
        servicios.Replace(ServiceDescriptor.Scoped<IRepositorioAlianza, RepositorioAlianzaPostgres>());
        servicios.Replace(ServiceDescriptor.Scoped<IRepositorioAnPrograma, RepositorioAnProgramaPostgres>());
        servicios.Replace(ServiceDescriptor.Scoped<IRepositorioAreaConocimiento, RepositorioAreaConocimientoPostgres>());
        servicios.Replace(ServiceDescriptor.Scoped<IRepositorioAspectoNormativo, RepositorioAspectoNormativoPostgres>());
        servicios.Replace(ServiceDescriptor.Scoped<IRepositorioCarInnovacion, RepositorioCarInnovacionPostgres>());
        servicios.Replace(ServiceDescriptor.Scoped<IRepositorioDashboard, RepositorioDashboardPostgres>());
        servicios.Replace(ServiceDescriptor.Scoped<IRepositorioDocenteDepartamento, RepositorioDocenteDepartamentoPostgres>());
        servicios.Replace(ServiceDescriptor.Scoped<IRepositorioEnfoqueRc, RepositorioEnfoqueRcPostgres>());
        servicios.Replace(ServiceDescriptor.Scoped<IRepositorioEnfoque, RepositorioEnfoquePostgres>());
        servicios.Replace(ServiceDescriptor.Scoped<IRepositorioFacultad, RepositorioFacultadPostgres>());
        servicios.Replace(ServiceDescriptor.Scoped<IRepositorioPasantia, RepositorioPasantiaPostgres>());
        servicios.Replace(ServiceDescriptor.Scoped<IRepositorioPracticaEstrategia, RepositorioPracticaEstrategiaPostgres>());
        servicios.Replace(ServiceDescriptor.Scoped<IRepositorioPremio, RepositorioPremioPostgres>());
        servicios.Replace(ServiceDescriptor.Scoped<IRepositorioProgramaAc, RepositorioProgramaAcPostgres>());
        servicios.Replace(ServiceDescriptor.Scoped<IRepositorioProgramaCi, RepositorioProgramaCiPostgres>());
        servicios.Replace(ServiceDescriptor.Scoped<IRepositorioProgramaCompleto, RepositorioProgramaCompletoPostgres>());
        servicios.Replace(ServiceDescriptor.Scoped<IRepositorioProgramaPe, RepositorioProgramaPePostgres>());
        servicios.Replace(ServiceDescriptor.Scoped<IRepositorioPrograma, RepositorioProgramaPostgres>());
        servicios.Replace(ServiceDescriptor.Scoped<IRepositorioReferencias, RepositorioReferenciasPostgres>());
        servicios.Replace(ServiceDescriptor.Scoped<IRepositorioRegistroCalificadoCompleto, RepositorioRegistroCalificadoCompletoPostgres>());
        servicios.Replace(ServiceDescriptor.Scoped<IRepositorioRegistroCalificado, RepositorioRegistroCalificadoPostgres>());
        servicios.Replace(ServiceDescriptor.Scoped<IRepositorioReportes, RepositorioReportesPostgres>());
        servicios.Replace(ServiceDescriptor.Scoped<IRepositorioRol, RepositorioRolPostgres>());
        servicios.Replace(ServiceDescriptor.Scoped<IRepositorioRolUsuario, RepositorioRolUsuarioPostgres>());
        servicios.Replace(ServiceDescriptor.Scoped<IRepositorioUniversidad, RepositorioUniversidadPostgres>());
        servicios.Replace(ServiceDescriptor.Scoped<IRepositorioUsuario, RepositorioUsuarioPostgres>());
    }
}
