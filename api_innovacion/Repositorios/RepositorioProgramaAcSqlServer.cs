using System.Data;
using ApiInnovacion.Excepciones;
using ApiInnovacion.Modelos;
using Dapper;
using Microsoft.Data.SqlClient;

namespace ApiInnovacion.Repositorios;

/// <summary>
/// La capa 3 contra SQL Server, con Dapper, para programa_ac. El SQL se escribe
/// a mano, queda a la vista y SIEMPRE va parametrizado (@parametro).
///
/// Todas las consultas filtran por activo = 1: el borrado es lógico.
/// </summary>
public class RepositorioProgramaAcSqlServer : IRepositorioProgramaAc
{
    private readonly string _cadenaConexion;

    public RepositorioProgramaAcSqlServer(IConfiguration configuracion)
    {
        _cadenaConexion = configuracion.GetConnectionString("SqlServer")
            ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'SqlServer'.");
    }

    private IDbConnection CrearConexion() => new SqlConnection(_cadenaConexion);

    // Los alias traducen snake_case (tabla) a PascalCase (entidad): Dapper
    // mapea por nombre.
    private const string COLUMNAS = @"programa AS Programa, area_conocimiento AS AreaConocimiento";

    public async Task<IEnumerable<ProgramaAc>> ObtenerTodos(int limite)
    {
        using var conexion = CrearConexion();
        var sql = $@"
            SELECT TOP (@Limite) {COLUMNAS}
            FROM programa_ac
            WHERE activo = 1
            ORDER BY programa ASC, area_conocimiento ASC";

        return await conexion.QueryAsync<ProgramaAc>(sql, new { Limite = limite });
    }

    public async Task<ProgramaAc?> ObtenerPorLlave(int programa, string areaConocimiento)
    {
        using var conexion = CrearConexion();
        // Un registro inactivo responde como inexistente.
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
        catch (SqlException ex) when (ex.Number is 2627 or 2601)
        {
            // 2627/2601: violación de llave primaria o índice único. Se traduce
            // aquí para que el servicio nunca vea un tipo de SqlClient.
            throw new LlaveDuplicadaExcepcion(
                "Ya existe un registro de programa_ac con esa llave (puede estar retirado).");
        }
    }

    public async Task<int> EliminarLogico(int programa, string areaConocimiento)
    {
        using var conexion = CrearConexion();
        // Borrado LÓGICO en una sola consulta: cero filas = no existe o ya
        // estaba inactivo, que es el 404 del contrato.
        const string sql = @"
            UPDATE programa_ac
            SET activo = 0
            WHERE programa = @Programa AND area_conocimiento = @AreaConocimiento AND activo = 1";

        return await conexion.ExecuteAsync(sql, new { Programa = programa, AreaConocimiento = areaConocimiento });
    }
}
