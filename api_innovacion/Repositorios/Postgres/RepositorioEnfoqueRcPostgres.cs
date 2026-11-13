// PostgreSQL: consultas explícitas; conserva el contrato del repositorio SQL Server.
using System.Data;
using ApiInnovacion.Excepciones;
using ApiInnovacion.Modelos;
using Dapper;
using Npgsql;

namespace ApiInnovacion.Repositorios.Postgres;

public class RepositorioEnfoqueRcPostgres : IRepositorioEnfoqueRc
{
    private readonly string _cadenaConexion;

    public RepositorioEnfoqueRcPostgres(ConfiguracionPostgres configuracion)
    {
        _cadenaConexion = configuracion.CadenaConexion;
    }

    private IDbConnection CrearConexion() => new NpgsqlConnection(_cadenaConexion);

    private const string COLUMNAS = @"enfoque AS Enfoque, registro_calificado AS RegistroCalificado";

    public async Task<IEnumerable<EnfoqueRc>> ObtenerTodos(int limite)
    {
        using var conexion = CrearConexion();
        var sql = $@"
            SELECT {COLUMNAS}
            FROM enfoque_rc
            WHERE activo = 1
            ORDER BY enfoque ASC, registro_calificado ASC LIMIT @Limite";

        return await conexion.QueryAsync<EnfoqueRc>(sql, new { Limite = limite });
    }

    public async Task<EnfoqueRc?> ObtenerPorLlave(int enfoque, int registroCalificado)
    {
        using var conexion = CrearConexion();

        var sql = $@"
            SELECT {COLUMNAS}
            FROM enfoque_rc
            WHERE enfoque = @Enfoque AND registro_calificado = @RegistroCalificado AND activo = 1";

        return await conexion.QueryFirstOrDefaultAsync<EnfoqueRc>(sql, new { Enfoque = enfoque, RegistroCalificado = registroCalificado });
    }

    public async Task Crear(EnfoqueRc entidad)
    {
        using var conexion = CrearConexion();
        const string sql = @"
            INSERT INTO enfoque_rc (enfoque, registro_calificado, activo)
            VALUES (@Enfoque, @RegistroCalificado, 1)";

        try
        {
            await conexion.ExecuteAsync(sql, entidad);
        }
        catch (PostgresException ex) when (ex.SqlState == "23505")
        {

            throw new LlaveDuplicadaExcepcion(
                "Ya existe un registro de enfoque_rc con esa llave (puede estar retirado).");
        }
    }

    public async Task<int> EliminarLogico(int enfoque, int registroCalificado)
    {
        using var conexion = CrearConexion();

        const string sql = @"
            UPDATE enfoque_rc
            SET activo = 0
            WHERE enfoque = @Enfoque AND registro_calificado = @RegistroCalificado AND activo = 1";

        return await conexion.ExecuteAsync(sql, new { Enfoque = enfoque, RegistroCalificado = registroCalificado });
    }
}
