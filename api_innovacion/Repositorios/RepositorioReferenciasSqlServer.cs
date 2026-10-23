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

    private async Task<IReadOnlyList<string>> Dependientes(string sql, object parametros)
    {
        using var conexion = CrearConexion();
        var conteos = await conexion.QueryAsync<Conteo>(sql, parametros);
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
        Dependientes(@"
            SELECT 'facultad' AS Tabla, COUNT(*) AS Cantidad FROM facultad WHERE universidad = @Id AND activo = 1",
            new { Id = id });

    public Task<IReadOnlyList<string>> DependientesDeFacultad(int id) =>
        Dependientes(@"
            SELECT 'programa' AS Tabla, COUNT(*) AS Cantidad FROM programa WHERE facultad = @Id AND activo = 1",
            new { Id = id });

    // programa es el centro del modelo: once tablas apuntan a él.
    public Task<IReadOnlyList<string>> DependientesDePrograma(int id) =>
        Dependientes(@"
            SELECT 'acreditacion' AS Tabla, COUNT(*) AS Cantidad FROM acreditacion WHERE programa = @Id AND activo = 1
            UNION ALL SELECT 'registro_calificado', COUNT(*) FROM registro_calificado WHERE programa = @Id AND activo = 1
            UNION ALL SELECT 'activ_academica', COUNT(*) FROM activ_academica WHERE disenio = @Id AND activo = 1
            UNION ALL SELECT 'pasantia', COUNT(*) FROM pasantia WHERE programa = @Id AND activo = 1
            UNION ALL SELECT 'premio', COUNT(*) FROM premio WHERE programa = @Id AND activo = 1
            UNION ALL SELECT 'programa_ac', COUNT(*) FROM programa_ac WHERE programa = @Id AND activo = 1
            UNION ALL SELECT 'programa_pe', COUNT(*) FROM programa_pe WHERE programa = @Id AND activo = 1
            UNION ALL SELECT 'programa_ci', COUNT(*) FROM programa_ci WHERE programa = @Id AND activo = 1
            UNION ALL SELECT 'an_programa', COUNT(*) FROM an_programa WHERE programa = @Id AND activo = 1
            UNION ALL SELECT 'docente_departamento', COUNT(*) FROM docente_departamento WHERE departamento = @Id AND activo = 1
            UNION ALL SELECT 'alianza', COUNT(*) FROM alianza WHERE departamento = @Id AND activo = 1",
            new { Id = id });

    public Task<IReadOnlyList<string>> DependientesDeRegistroCalificado(int codigo) =>
        Dependientes(@"
            SELECT 'enfoque_rc' AS Tabla, COUNT(*) AS Cantidad FROM enfoque_rc WHERE registro_calificado = @Codigo AND activo = 1
            UNION ALL SELECT 'aa_rc', COUNT(*) FROM aa_rc WHERE registro_calificado_codigo = @Codigo AND activo = 1",
            new { Codigo = codigo });

    public Task<IReadOnlyList<string>> DependientesDeActivAcademica(int id) =>
        Dependientes(@"
            SELECT 'aa_rc' AS Tabla, COUNT(*) AS Cantidad FROM aa_rc WHERE activ_academicas_idcurso = @Id AND activo = 1",
            new { Id = id });

    public Task<IReadOnlyList<string>> DependientesDeAreaConocimiento(string id) =>
        Dependientes(@"
            SELECT 'programa_ac' AS Tabla, COUNT(*) AS Cantidad FROM programa_ac WHERE area_conocimiento = @Id AND activo = 1",
            new { Id = id });

    public Task<IReadOnlyList<string>> DependientesDePracticaEstrategia(int id) =>
        Dependientes(@"
            SELECT 'programa_pe' AS Tabla, COUNT(*) AS Cantidad FROM programa_pe WHERE practica_estrategia = @Id AND activo = 1",
            new { Id = id });

    public Task<IReadOnlyList<string>> DependientesDeCarInnovacion(int id) =>
        Dependientes(@"
            SELECT 'programa_ci' AS Tabla, COUNT(*) AS Cantidad FROM programa_ci WHERE car_innovacion = @Id AND activo = 1",
            new { Id = id });

    public Task<IReadOnlyList<string>> DependientesDeAspectoNormativo(int id) =>
        Dependientes(@"
            SELECT 'an_programa' AS Tabla, COUNT(*) AS Cantidad FROM an_programa WHERE aspecto_normativo = @Id AND activo = 1",
            new { Id = id });

    public Task<IReadOnlyList<string>> DependientesDeEnfoque(int id) =>
        Dependientes(@"
            SELECT 'enfoque_rc' AS Tabla, COUNT(*) AS Cantidad FROM enfoque_rc WHERE enfoque = @Id AND activo = 1",
            new { Id = id });

    public Task<IReadOnlyList<string>> DependientesDeAliado(int nit) =>
        Dependientes(@"
            SELECT 'alianza' AS Tabla, COUNT(*) AS Cantidad FROM alianza WHERE aliado = @Nit AND activo = 1",
            new { Nit = nit });
}
