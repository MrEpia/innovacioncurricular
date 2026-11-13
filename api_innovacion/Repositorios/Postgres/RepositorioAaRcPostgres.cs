// PostgreSQL: consultas explícitas; conserva el contrato del repositorio SQL Server.
using System.Data;
using ApiInnovacion.Excepciones;
using ApiInnovacion.Modelos;
using Dapper;
using Npgsql;

namespace ApiInnovacion.Repositorios.Postgres;

public class RepositorioAaRcPostgres : IRepositorioAaRc
{
    private readonly string _cadenaConexion;

    public RepositorioAaRcPostgres(ConfiguracionPostgres configuracion)
    {
        _cadenaConexion = configuracion.CadenaConexion;
    }

    private IDbConnection CrearConexion() => new NpgsqlConnection(_cadenaConexion);

    private const string COLUMNAS = @"activ_academicas_idcurso AS ActivAcademicasIdcurso, registro_calificado_codigo AS RegistroCalificadoCodigo, componente AS Componente, semestre AS Semestre";

    public async Task<IEnumerable<AaRc>> ObtenerTodos(int limite)
    {
        using var conexion = CrearConexion();
        var sql = $@"
            SELECT {COLUMNAS}
            FROM aa_rc
            WHERE activo = 1
            ORDER BY activ_academicas_idcurso ASC, registro_calificado_codigo ASC LIMIT @Limite";

        return await conexion.QueryAsync<AaRc>(sql, new { Limite = limite });
    }

    public async Task<AaRc?> ObtenerPorLlave(int activAcademicasIdcurso, int registroCalificadoCodigo)
    {
        using var conexion = CrearConexion();

        var sql = $@"
            SELECT {COLUMNAS}
            FROM aa_rc
            WHERE activ_academicas_idcurso = @ActivAcademicasIdcurso AND registro_calificado_codigo = @RegistroCalificadoCodigo AND activo = 1";

        return await conexion.QueryFirstOrDefaultAsync<AaRc>(sql, new { ActivAcademicasIdcurso = activAcademicasIdcurso, RegistroCalificadoCodigo = registroCalificadoCodigo });
    }

    public async Task Crear(AaRc entidad)
    {
        using var conexion = CrearConexion();
        const string sql = @"
            INSERT INTO aa_rc (activ_academicas_idcurso, registro_calificado_codigo, componente, semestre, activo)
            VALUES (@ActivAcademicasIdcurso, @RegistroCalificadoCodigo, @Componente, @Semestre, 1)";

        try
        {
            await conexion.ExecuteAsync(sql, entidad);
        }
        catch (PostgresException ex) when (ex.SqlState == "23505")
        {

            throw new LlaveDuplicadaExcepcion(
                "Ya existe un registro de aa_rc con esa llave (puede estar retirado).");
        }
    }

    public async Task<int> Reemplazar(AaRc entidad)
    {
        using var conexion = CrearConexion();
        const string sql = @"
            UPDATE aa_rc
            SET componente = @Componente, semestre = @Semestre
            WHERE activ_academicas_idcurso = @ActivAcademicasIdcurso AND registro_calificado_codigo = @RegistroCalificadoCodigo AND activo = 1";

        return await conexion.ExecuteAsync(sql, entidad);
    }

    public async Task<int> ActualizarParcial(int activAcademicasIdcurso, int registroCalificadoCodigo, AaRcCambios cambios)
    {
        using var conexion = CrearConexion();

        var asignaciones = new List<string>();
        var parametros = new DynamicParameters();
        parametros.Add("ActivAcademicasIdcurso", activAcademicasIdcurso);
        parametros.Add("RegistroCalificadoCodigo", registroCalificadoCodigo);

        if (cambios.Componente != null)
        {
            asignaciones.Add("componente = @Componente");
            parametros.Add("Componente", cambios.Componente);
        }
        if (cambios.Semestre != null)
        {
            asignaciones.Add("semestre = @Semestre");
            parametros.Add("Semestre", cambios.Semestre);
        }

        if (asignaciones.Count == 0) return 0;

        var sql = $"UPDATE aa_rc SET {string.Join(", ", asignaciones)} WHERE activ_academicas_idcurso = @ActivAcademicasIdcurso AND registro_calificado_codigo = @RegistroCalificadoCodigo AND activo = 1";

        return await conexion.ExecuteAsync(sql, parametros);
    }

    public async Task<int> EliminarLogico(int activAcademicasIdcurso, int registroCalificadoCodigo)
    {
        using var conexion = CrearConexion();

        const string sql = @"
            UPDATE aa_rc
            SET activo = 0
            WHERE activ_academicas_idcurso = @ActivAcademicasIdcurso AND registro_calificado_codigo = @RegistroCalificadoCodigo AND activo = 1";

        return await conexion.ExecuteAsync(sql, new { ActivAcademicasIdcurso = activAcademicasIdcurso, RegistroCalificadoCodigo = registroCalificadoCodigo });
    }
}
