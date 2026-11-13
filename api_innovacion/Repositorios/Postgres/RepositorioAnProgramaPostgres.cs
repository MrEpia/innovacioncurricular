// PostgreSQL: consultas explícitas; conserva el contrato del repositorio SQL Server.
using System.Data;
using ApiInnovacion.Excepciones;
using ApiInnovacion.Modelos;
using Dapper;
using Npgsql;

namespace ApiInnovacion.Repositorios.Postgres;

public class RepositorioAnProgramaPostgres : IRepositorioAnPrograma
{
    private readonly string _cadenaConexion;

    public RepositorioAnProgramaPostgres(ConfiguracionPostgres configuracion)
    {
        _cadenaConexion = configuracion.CadenaConexion;
    }

    private IDbConnection CrearConexion() => new NpgsqlConnection(_cadenaConexion);

    private const string COLUMNAS = @"aspecto_normativo AS AspectoNormativo, programa AS Programa";

    public async Task<IEnumerable<AnPrograma>> ObtenerTodos(int limite)
    {
        using var conexion = CrearConexion();
        var sql = $@"
            SELECT {COLUMNAS}
            FROM an_programa
            WHERE activo = 1
            ORDER BY aspecto_normativo ASC, programa ASC LIMIT @Limite";

        return await conexion.QueryAsync<AnPrograma>(sql, new { Limite = limite });
    }

    public async Task<AnPrograma?> ObtenerPorLlave(int aspectoNormativo, int programa)
    {
        using var conexion = CrearConexion();

        var sql = $@"
            SELECT {COLUMNAS}
            FROM an_programa
            WHERE aspecto_normativo = @AspectoNormativo AND programa = @Programa AND activo = 1";

        return await conexion.QueryFirstOrDefaultAsync<AnPrograma>(sql, new { AspectoNormativo = aspectoNormativo, Programa = programa });
    }

    public async Task Crear(AnPrograma entidad)
    {
        using var conexion = CrearConexion();
        const string sql = @"
            INSERT INTO an_programa (aspecto_normativo, programa, activo)
            VALUES (@AspectoNormativo, @Programa, 1)";

        try
        {
            await conexion.ExecuteAsync(sql, entidad);
        }
        catch (PostgresException ex) when (ex.SqlState == "23505")
        {

            throw new LlaveDuplicadaExcepcion(
                "Ya existe un registro de an_programa con esa llave (puede estar retirado).");
        }
    }

    public async Task<int> EliminarLogico(int aspectoNormativo, int programa)
    {
        using var conexion = CrearConexion();

        const string sql = @"
            UPDATE an_programa
            SET activo = 0
            WHERE aspecto_normativo = @AspectoNormativo AND programa = @Programa AND activo = 1";

        return await conexion.ExecuteAsync(sql, new { AspectoNormativo = aspectoNormativo, Programa = programa });
    }
}
