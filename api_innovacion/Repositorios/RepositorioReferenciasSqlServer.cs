using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;

namespace ApiInnovacion.Repositorios;

/// <summary>
/// Las preguntas de integridad referencial contra SQL Server. Cada consulta está
/// escrita completa y parametrizada: los nombres de tabla son parte del SQL,
/// nunca un valor que llega de afuera.
///
/// Todas filtran activo = 1: un padre inactivo NO cuenta como existente, y un
/// hijo inactivo NO impide retirar al padre.
/// </summary>
public class RepositorioReferenciasSqlServer : IRepositorioReferencias
{
    private readonly string _cadenaConexion;

    public RepositorioReferenciasSqlServer(IConfiguration configuracion)
    {
        _cadenaConexion = configuracion.GetConnectionString("SqlServer")
            ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'SqlServer'.");
    }

    private IDbConnection CrearConexion() => new SqlConnection(_cadenaConexion);

    private async Task<bool> Existe(string sql, object parametros)
    {
        using var conexion = CrearConexion();
        return await conexion.ExecuteScalarAsync<int>(sql, parametros) > 0;
    }

    /// <summary>Una fila por tabla dependiente: (Tabla, Cantidad).</summary>
    private sealed class Conteo
    {
        public string Tabla { get; set; } = string.Empty;
        public int Cantidad { get; set; }
    }

    /// <summary>
    /// Quién depende de quién vive en los procedimientos sp_dependientes_&lt;tabla&gt;
    /// (migración v3_002): los mismos que usa sp_retirar_&lt;tabla&gt; para retirar
    /// sin dejar huérfanos, así la regla está escrita una sola vez.
    /// </summary>
    private async Task<IReadOnlyList<string>> Dependientes(string procedimiento, object parametros)
    {
        using var conexion = CrearConexion();
        var conteos = await conexion.QueryAsync<Conteo>(
            procedimiento, parametros, commandType: CommandType.StoredProcedure);
        return conteos
            .Where(c => c.Cantidad > 0)
            .Select(c => $"{c.Tabla} ({c.Cantidad})")
            .ToList();
    }

    // ===================== 1. ¿Existe y está activo? =====================

    public Task<bool> UniversidadActiva(int id) =>
        Existe("SELECT COUNT(*) FROM universidad WHERE id = @Id AND activo = 1", new { Id = id });

    public Task<bool> FacultadActiva(int id) =>
        Existe("SELECT COUNT(*) FROM facultad WHERE id = @Id AND activo = 1", new { Id = id });

    public Task<bool> ProgramaActivo(int id) =>
        Existe("SELECT COUNT(*) FROM programa WHERE id = @Id AND activo = 1", new { Id = id });

    public Task<bool> RegistroCalificadoActivo(int codigo) =>
        Existe("SELECT COUNT(*) FROM registro_calificado WHERE codigo = @Codigo AND activo = 1", new { Codigo = codigo });

    public Task<bool> ActivAcademicaActiva(int id) =>
        Existe("SELECT COUNT(*) FROM activ_academica WHERE id = @Id AND activo = 1", new { Id = id });

    public Task<bool> AreaConocimientoActiva(string id) =>
        Existe("SELECT COUNT(*) FROM area_conocimiento WHERE id = @Id AND activo = 1", new { Id = id });

    public Task<bool> PracticaEstrategiaActiva(int id) =>
        Existe("SELECT COUNT(*) FROM practica_estrategia WHERE id = @Id AND activo = 1", new { Id = id });

    public Task<bool> CarInnovacionActiva(int id) =>
        Existe("SELECT COUNT(*) FROM car_innovacion WHERE id = @Id AND activo = 1", new { Id = id });

    public Task<bool> AspectoNormativoActivo(int id) =>
        Existe("SELECT COUNT(*) FROM aspecto_normativo WHERE id = @Id AND activo = 1", new { Id = id });

    public Task<bool> EnfoqueActivo(int id) =>
        Existe("SELECT COUNT(*) FROM enfoque WHERE id = @Id AND activo = 1", new { Id = id });

    public Task<bool> AliadoActivo(int nit) =>
        Existe("SELECT COUNT(*) FROM aliado WHERE nit = @Nit AND activo = 1", new { Nit = nit });

    public Task<bool> UsuarioActivo(int id) =>
        Existe("SELECT COUNT(*) FROM usuario WHERE id = @Id AND activo = 1", new { Id = id });

    public Task<bool> RolActivo(int id) =>
        Existe("SELECT COUNT(*) FROM rol WHERE id = @Id AND activo = 1", new { Id = id });

    // ===================== 2. ¿Quién depende de él? =====================

    public Task<IReadOnlyList<string>> DependientesDeUniversidad(int id) =>
        Dependientes("dbo.sp_dependientes_universidad", new { Id = id });

    public Task<IReadOnlyList<string>> DependientesDeFacultad(int id) =>
        Dependientes("dbo.sp_dependientes_facultad", new { Id = id });

    // programa es el centro del modelo: once tablas apuntan a él.
    public Task<IReadOnlyList<string>> DependientesDePrograma(int id) =>
        Dependientes("dbo.sp_dependientes_programa", new { Id = id });

    public Task<IReadOnlyList<string>> DependientesDeRegistroCalificado(int codigo) =>
        Dependientes("dbo.sp_dependientes_registro_calificado", new { Codigo = codigo });

    public Task<IReadOnlyList<string>> DependientesDeActivAcademica(int id) =>
        Dependientes("dbo.sp_dependientes_activ_academica", new { Id = id });

    public Task<IReadOnlyList<string>> DependientesDeAreaConocimiento(string id) =>
        Dependientes("dbo.sp_dependientes_area_conocimiento", new { Id = id });

    public Task<IReadOnlyList<string>> DependientesDePracticaEstrategia(int id) =>
        Dependientes("dbo.sp_dependientes_practica_estrategia", new { Id = id });

    public Task<IReadOnlyList<string>> DependientesDeCarInnovacion(int id) =>
        Dependientes("dbo.sp_dependientes_car_innovacion", new { Id = id });

    public Task<IReadOnlyList<string>> DependientesDeAspectoNormativo(int id) =>
        Dependientes("dbo.sp_dependientes_aspecto_normativo", new { Id = id });

    public Task<IReadOnlyList<string>> DependientesDeEnfoque(int id) =>
        Dependientes("dbo.sp_dependientes_enfoque", new { Id = id });

    public Task<IReadOnlyList<string>> DependientesDeAliado(int nit) =>
        Dependientes("dbo.sp_dependientes_aliado", new { Nit = nit });
}
