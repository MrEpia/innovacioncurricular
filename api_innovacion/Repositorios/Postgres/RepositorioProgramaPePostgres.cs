// PostgreSQL: consultas explícitas; conserva el contrato del repositorio SQL Server.
using System.Data;
using ApiInnovacion.Excepciones;
using ApiInnovacion.Modelos;
using Dapper;
using Npgsql;

namespace ApiInnovacion.Repositorios.Postgres;

public class RepositorioProgramaPePostgres : IRepositorioProgramaPe
{
    private readonly string _cadenaConexion;

    public RepositorioProgramaPePostgres(ConfiguracionPostgres configuracion)
    {
        _cadenaConexion = configuracion.CadenaConexion;
    }

    private IDbConnection CrearConexion() => new NpgsqlConnection(_cadenaConexion);

    private const string COLUMNAS = @"programa AS Programa, practica_estrategia AS PracticaEstrategia";

    public async Task<IEnumerable<ProgramaPe>> ObtenerTodos(int limite)
    {
        using var conexion = CrearConexion();
        var sql = $@"
            SELECT {COLUMNAS}
            FROM programa_pe
            WHERE activo = 1
            ORDER BY programa ASC, practica_estrategia ASC LIMIT @Limite";

        return await conexion.QueryAsync<ProgramaPe>(sql, new { Limite = limite });
    }

    public async Task<ProgramaPe?> ObtenerPorLlave(int programa, int practicaEstrategia)
    {
        using var conexion = CrearConexion();

        var sql = $@"
            SELECT {COLUMNAS}
            FROM programa_pe
            WHERE programa = @Programa AND practica_estrategia = @PracticaEstrategia AND activo = 1";

        return await conexion.QueryFirstOrDefaultAsync<ProgramaPe>(sql, new { Programa = programa, PracticaEstrategia = practicaEstrategia });
    }

    public async Task Crear(ProgramaPe entidad)
    {
        using var conexion = CrearConexion();
        const string sql = @"
            INSERT INTO programa_pe (programa, practica_estrategia, activo)
            VALUES (@Programa, @PracticaEstrategia, 1)";

        try
        {
            await conexion.ExecuteAsync(sql, entidad);
        }
        catch (PostgresException ex) when (ex.SqlState == "23505")
        {

            throw new LlaveDuplicadaExcepcion(
                "Ya existe un registro de programa_pe con esa llave (puede estar retirado).");
        }
    }

    public async Task<int> EliminarLogico(int programa, int practicaEstrategia)
    {
        using var conexion = CrearConexion();

        const string sql = @"
            UPDATE programa_pe
            SET activo = 0
            WHERE programa = @Programa AND practica_estrategia = @PracticaEstrategia AND activo = 1";

        return await conexion.ExecuteAsync(sql, new { Programa = programa, PracticaEstrategia = practicaEstrategia });
    }
}
