using System.Data;
using ApiInnovacion.Excepciones;
using ApiInnovacion.Modelos;
using Dapper;
using Microsoft.Data.SqlClient;

namespace ApiInnovacion.Repositorios;

/// <summary>
/// La capa 3 contra SQL Server, con Dapper, para enfoque_rc. El SQL se escribe
/// a mano, queda a la vista y SIEMPRE va parametrizado (@parametro).
///
/// Todas las consultas filtran por activo = 1: el borrado es lógico.
/// </summary>
public class RepositorioEnfoqueRcSqlServer : IRepositorioEnfoqueRc
{
    private readonly string _cadenaConexion;

    public RepositorioEnfoqueRcSqlServer(IConfiguration configuracion)
    {
        _cadenaConexion = configuracion.GetConnectionString("SqlServer")
            ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'SqlServer'.");
    }

    private IDbConnection CrearConexion() => new SqlConnection(_cadenaConexion);

    // Los alias traducen snake_case (tabla) a PascalCase (entidad): Dapper
    // mapea por nombre.
    private const string COLUMNAS = @"enfoque AS Enfoque, registro_calificado AS RegistroCalificado";

    public async Task<IEnumerable<EnfoqueRc>> ObtenerTodos(int limite)
    {
        using var conexion = CrearConexion();
        var sql = $@"
            SELECT TOP (@Limite) {COLUMNAS}
            FROM enfoque_rc
            WHERE activo = 1
            ORDER BY enfoque ASC, registro_calificado ASC";

        return await conexion.QueryAsync<EnfoqueRc>(sql, new { Limite = limite });
    }

    public async Task<EnfoqueRc?> ObtenerPorLlave(int enfoque, int registroCalificado)
    {
        using var conexion = CrearConexion();
        // Un registro inactivo responde como inexistente.
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
        catch (SqlException ex) when (ex.Number is 2627 or 2601)
        {
            // 2627/2601: violación de llave primaria o índice único. Se traduce
            // aquí para que el servicio nunca vea un tipo de SqlClient.
            throw new LlaveDuplicadaExcepcion(
                "Ya existe un registro de enfoque_rc con esa llave (puede estar retirado).");
        }
    }

    public async Task<int> EliminarLogico(int enfoque, int registroCalificado)
    {
        using var conexion = CrearConexion();
        // Borrado LÓGICO en una sola consulta: cero filas = no existe o ya
        // estaba inactivo, que es el 404 del contrato.
        const string sql = @"
            UPDATE enfoque_rc
            SET activo = 0
            WHERE enfoque = @Enfoque AND registro_calificado = @RegistroCalificado AND activo = 1";

        return await conexion.ExecuteAsync(sql, new { Enfoque = enfoque, RegistroCalificado = registroCalificado });
    }
}
