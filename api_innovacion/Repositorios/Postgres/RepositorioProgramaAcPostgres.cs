// PostgreSQL: consultas explícitas; conserva el contrato del repositorio SQL Server.
using System.Data;
using ApiInnovacion.Excepciones;
using ApiInnovacion.Modelos;
using Dapper;
using Npgsql;

namespace ApiInnovacion.Repositorios.Postgres;

public class RepositorioProgramaAcPostgres : IRepositorioProgramaAc
{
    private readonly string _cadenaConexion;

    public RepositorioProgramaAcPostgres(ConfiguracionPostgres configuracion)
    {
        _cadenaConexion = configuracion.CadenaConexion;
    }

    private IDbConnection CrearConexion() => new NpgsqlConnection(_cadenaConexion);

    private const string COLUMNAS = @"programa AS Programa, area_conocimiento AS AreaConocimiento";

    public async Task<IEnumerable<ProgramaAc>> ObtenerTodos(int limite)
    {
        using var conexion = CrearConexion();
        var sql = $@"
            SELECT {COLUMNAS}
            FROM programa_ac
            WHERE activo = 1
            ORDER BY programa ASC, area_conocimiento ASC LIMIT @Limite";

        return await conexion.QueryAsync<ProgramaAc>(sql, new { Limite = limite });
    }

    public async Task<ProgramaAc?> ObtenerPorLlave(int programa, string areaConocimiento)
    {
        using var conexion = CrearConexion();

        var sql = $@"
            SELECT {COLUMNAS}
            FROM programa_ac
            WHERE programa = @Programa AND area_conocimiento = @AreaConocimiento AND activo = 1";

        return await conexion.QueryFirstOrDefaultAsync<ProgramaAc>(sql, new { Programa = programa, AreaConocimiento = areaConocimiento });
    }

    public async Task Crear(ProgramaAc entidad)
    {
        using var conexion = CrearConexion();
        const string sql = @"
            INSERT INTO programa_ac (programa, area_conocimiento, activo)
            VALUES (@Programa, @AreaConocimiento, 1)";

        try
        {
            await conexion.ExecuteAsync(sql, entidad);
        }
        catch (PostgresException ex) when (ex.SqlState == "23505")
        {

            throw new LlaveDuplicadaExcepcion(
                "Ya existe un registro de programa_ac con esa llave (puede estar retirado).");
        }
    }

    public async Task<int> EliminarLogico(int programa, string areaConocimiento)
    {
        using var conexion = CrearConexion();

        const string sql = @"
            UPDATE programa_ac
            SET activo = 0
            WHERE programa = @Programa AND area_conocimiento = @AreaConocimiento AND activo = 1";

        return await conexion.ExecuteAsync(sql, new { Programa = programa, AreaConocimiento = areaConocimiento });
    }
}
