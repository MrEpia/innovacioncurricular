// PostgreSQL: consultas explícitas; conserva el contrato del repositorio SQL Server.
using System.Data;
using ApiInnovacion.Excepciones;
using ApiInnovacion.Modelos;
using Dapper;
using Npgsql;

namespace ApiInnovacion.Repositorios.Postgres;

public class RepositorioProgramaCiPostgres : IRepositorioProgramaCi
{
    private readonly string _cadenaConexion;

    public RepositorioProgramaCiPostgres(ConfiguracionPostgres configuracion)
    {
        _cadenaConexion = configuracion.CadenaConexion;
    }

    private IDbConnection CrearConexion() => new NpgsqlConnection(_cadenaConexion);

    private const string COLUMNAS = @"programa AS Programa, car_innovacion AS CarInnovacion";

    public async Task<IEnumerable<ProgramaCi>> ObtenerTodos(int limite)
    {
        using var conexion = CrearConexion();
        var sql = $@"
            SELECT {COLUMNAS}
            FROM programa_ci
            WHERE activo = 1
            ORDER BY programa ASC, car_innovacion ASC LIMIT @Limite";

        return await conexion.QueryAsync<ProgramaCi>(sql, new { Limite = limite });
    }

    public async Task<ProgramaCi?> ObtenerPorLlave(int programa, int carInnovacion)
    {
        using var conexion = CrearConexion();

        var sql = $@"
            SELECT {COLUMNAS}
            FROM programa_ci
            WHERE programa = @Programa AND car_innovacion = @CarInnovacion AND activo = 1";

        return await conexion.QueryFirstOrDefaultAsync<ProgramaCi>(sql, new { Programa = programa, CarInnovacion = carInnovacion });
    }

    public async Task Crear(ProgramaCi entidad)
    {
        using var conexion = CrearConexion();
        const string sql = @"
            INSERT INTO programa_ci (programa, car_innovacion, activo)
            VALUES (@Programa, @CarInnovacion, 1)";

        try
        {
            await conexion.ExecuteAsync(sql, entidad);
        }
        catch (PostgresException ex) when (ex.SqlState == "23505")
        {

            throw new LlaveDuplicadaExcepcion(
                "Ya existe un registro de programa_ci con esa llave (puede estar retirado).");
        }
    }

    public async Task<int> EliminarLogico(int programa, int carInnovacion)
    {
        using var conexion = CrearConexion();

        const string sql = @"
            UPDATE programa_ci
            SET activo = 0
            WHERE programa = @Programa AND car_innovacion = @CarInnovacion AND activo = 1";

        return await conexion.ExecuteAsync(sql, new { Programa = programa, CarInnovacion = carInnovacion });
    }
}
