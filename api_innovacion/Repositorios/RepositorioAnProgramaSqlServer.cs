using System.Data;
using ApiInnovacion.Excepciones;
using ApiInnovacion.Modelos;
using Dapper;
using Microsoft.Data.SqlClient;

namespace ApiInnovacion.Repositorios;

/// <summary>
/// La capa 3 contra SQL Server, con Dapper, para an_programa. El SQL se escribe
/// a mano, queda a la vista y SIEMPRE va parametrizado (@parametro).
///
/// Todas las consultas filtran por activo = 1: el borrado es lógico.
/// </summary>
public class RepositorioAnProgramaSqlServer : IRepositorioAnPrograma
{
    private readonly string _cadenaConexion;

    public RepositorioAnProgramaSqlServer(IConfiguration configuracion)
    {
        _cadenaConexion = configuracion.GetConnectionString("SqlServer")
            ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'SqlServer'.");
    }

    private IDbConnection CrearConexion() => new SqlConnection(_cadenaConexion);

    // Los alias traducen snake_case (tabla) a PascalCase (entidad): Dapper
    // mapea por nombre.
    private const string COLUMNAS = @"aspecto_normativo AS AspectoNormativo, programa AS Programa";

    public async Task<IEnumerable<AnPrograma>> ObtenerTodos(int limite)
    {
        using var conexion = CrearConexion();
        var sql = $@"
            SELECT TOP (@Limite) {COLUMNAS}
            FROM an_programa
            WHERE activo = 1
            ORDER BY aspecto_normativo ASC, programa ASC";

        return await conexion.QueryAsync<AnPrograma>(sql, new { Limite = limite });
    }

    public async Task<AnPrograma?> ObtenerPorLlave(int aspectoNormativo, int programa)
    {
        using var conexion = CrearConexion();
        // Un registro inactivo responde como inexistente.
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
        catch (SqlException ex) when (ex.Number is 2627 or 2601)
        {
            // 2627/2601: violación de llave primaria o índice único. Se traduce
            // aquí para que el servicio nunca vea un tipo de SqlClient.
            throw new LlaveDuplicadaExcepcion(
                "Ya existe un registro de an_programa con esa llave (puede estar retirado).");
        }
    }

    public async Task<int> EliminarLogico(int aspectoNormativo, int programa)
    {
        using var conexion = CrearConexion();
        // Borrado LÓGICO en una sola consulta: cero filas = no existe o ya
        // estaba inactivo, que es el 404 del contrato.
        const string sql = @"
            UPDATE an_programa
            SET activo = 0
            WHERE aspecto_normativo = @AspectoNormativo AND programa = @Programa AND activo = 1";

        return await conexion.ExecuteAsync(sql, new { AspectoNormativo = aspectoNormativo, Programa = programa });
    }
}
