using System.Data;
using ApiInnovacion.Excepciones;
using ApiInnovacion.Modelos;
using Dapper;
using Microsoft.Data.SqlClient;

namespace ApiInnovacion.Repositorios;

/// <summary>
/// La capa 3 contra SQL Server, con Dapper, para programa_ci. El SQL se escribe
/// a mano, queda a la vista y SIEMPRE va parametrizado (@parametro).
///
/// Todas las consultas filtran por activo = 1: el borrado es lógico.
/// </summary>
public class RepositorioProgramaCiSqlServer : IRepositorioProgramaCi
{
    private readonly string _cadenaConexion;

    public RepositorioProgramaCiSqlServer(IConfiguration configuracion)
    {
        _cadenaConexion = configuracion.GetConnectionString("SqlServer")
            ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'SqlServer'.");
    }

    private IDbConnection CrearConexion() => new SqlConnection(_cadenaConexion);

    // Los alias traducen snake_case (tabla) a PascalCase (entidad): Dapper
    // mapea por nombre.
    private const string COLUMNAS = @"programa AS Programa, car_innovacion AS CarInnovacion";

    public async Task<IEnumerable<ProgramaCi>> ObtenerTodos(int limite)
    {
        using var conexion = CrearConexion();
        var sql = $@"
            SELECT TOP (@Limite) {COLUMNAS}
            FROM programa_ci
            WHERE activo = 1
            ORDER BY programa ASC, car_innovacion ASC";

        return await conexion.QueryAsync<ProgramaCi>(sql, new { Limite = limite });
    }

    public async Task<ProgramaCi?> ObtenerPorLlave(int programa, int carInnovacion)
    {
        using var conexion = CrearConexion();
        // Un registro inactivo responde como inexistente.
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
        catch (SqlException ex) when (ex.Number is 2627 or 2601)
        {
            // 2627/2601: violación de llave primaria o índice único. Se traduce
            // aquí para que el servicio nunca vea un tipo de SqlClient.
            throw new LlaveDuplicadaExcepcion(
                "Ya existe un registro de programa_ci con esa llave (puede estar retirado).");
        }
    }

    public async Task<int> EliminarLogico(int programa, int carInnovacion)
    {
        using var conexion = CrearConexion();
        // Borrado LÓGICO en una sola consulta: cero filas = no existe o ya
        // estaba inactivo, que es el 404 del contrato.
        const string sql = @"
            UPDATE programa_ci
            SET activo = 0
            WHERE programa = @Programa AND car_innovacion = @CarInnovacion AND activo = 1";

        return await conexion.ExecuteAsync(sql, new { Programa = programa, CarInnovacion = carInnovacion });
    }
}
