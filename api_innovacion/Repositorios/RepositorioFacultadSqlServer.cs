using System.Data;
using ApiInnovacion.Excepciones;
using ApiInnovacion.Modelos;
using Dapper;
using Microsoft.Data.SqlClient;

namespace ApiInnovacion.Repositorios;

/// <summary>
/// La capa 3 contra SQL Server, con Dapper, para facultad. El SQL se escribe
/// a mano, queda a la vista y SIEMPRE va parametrizado (@parametro).
///
/// Todas las consultas filtran por activo = 1: el borrado es lógico.
/// </summary>
public class RepositorioFacultadSqlServer : IRepositorioFacultad
{
    private readonly string _cadenaConexion;

    public RepositorioFacultadSqlServer(IConfiguration configuracion)
    {
        _cadenaConexion = configuracion.GetConnectionString("SqlServer")
            ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'SqlServer'.");
    }

    private IDbConnection CrearConexion() => new SqlConnection(_cadenaConexion);

    // Los alias traducen snake_case (tabla) a PascalCase (entidad): Dapper
    // mapea por nombre.
    private const string COLUMNAS = @"id AS Id, nombre AS Nombre, tipo AS Tipo, fecha_fun AS FechaFun, universidad AS Universidad";

    public async Task<IEnumerable<Facultad>> ObtenerTodos(int limite)
    {
        using var conexion = CrearConexion();
        var sql = $@"
            SELECT TOP (@Limite) {COLUMNAS}
            FROM facultad
            WHERE activo = 1
            ORDER BY id ASC";

        return await conexion.QueryAsync<Facultad>(sql, new { Limite = limite });
    }

    public async Task<Facultad?> ObtenerPorId(int id)
    {
        using var conexion = CrearConexion();
        // Un registro inactivo responde como inexistente.
        var sql = $@"
            SELECT {COLUMNAS}
            FROM facultad
            WHERE id = @Id AND activo = 1";

        return await conexion.QueryFirstOrDefaultAsync<Facultad>(sql, new { Id = id });
    }

    public async Task Crear(Facultad entidad)
    {
        using var conexion = CrearConexion();
        const string sql = @"
            INSERT INTO facultad (id, nombre, tipo, fecha_fun, universidad, activo)
            VALUES (@Id, @Nombre, @Tipo, @FechaFun, @Universidad, 1)";

        try
        {
            await conexion.ExecuteAsync(sql, entidad);
        }
        catch (SqlException ex) when (ex.Number is 2627 or 2601)
        {
            // 2627/2601: violación de llave primaria o índice único. Se traduce
            // aquí para que el servicio nunca vea un tipo de SqlClient.
            throw new LlaveDuplicadaExcepcion(
                "Ya existe un registro de facultad con esa llave (puede estar retirado).");
        }
    }

    public async Task<int> Reemplazar(Facultad entidad)
    {
        using var conexion = CrearConexion();
        const string sql = @"
            UPDATE facultad
            SET nombre = @Nombre, tipo = @Tipo, fecha_fun = @FechaFun, universidad = @Universidad
            WHERE id = @Id AND activo = 1";

        return await conexion.ExecuteAsync(sql, entidad);
    }

    public async Task<int> ActualizarParcial(int id, FacultadCambios cambios)
    {
        using var conexion = CrearConexion();

        // El PATCH escribe solo lo que llegó, así que la consulta se compone.
        // Lo que se compone son NOMBRES DE COLUMNA de una lista cerrada escrita
        // aquí; los VALORES siempre viajan como @parametro.
        var asignaciones = new List<string>();
        var parametros = new DynamicParameters();
        parametros.Add("Id", id);

        if (cambios.Nombre != null)
        {
            asignaciones.Add("nombre = @Nombre");
            parametros.Add("Nombre", cambios.Nombre);
        }
        if (cambios.Tipo != null)
        {
            asignaciones.Add("tipo = @Tipo");
            parametros.Add("Tipo", cambios.Tipo);
        }
        if (cambios.FechaFun != null)
        {
            asignaciones.Add("fecha_fun = @FechaFun");
            parametros.Add("FechaFun", cambios.FechaFun);
        }
        if (cambios.Universidad != null)
        {
            asignaciones.Add("universidad = @Universidad");
            parametros.Add("Universidad", cambios.Universidad);
        }

        if (asignaciones.Count == 0) return 0;

        var sql = $"UPDATE facultad SET {string.Join(", ", asignaciones)} WHERE id = @Id AND activo = 1";

        return await conexion.ExecuteAsync(sql, parametros);
    }

    public async Task<int> EliminarLogico(int id)
    {
        using var conexion = CrearConexion();
        // Procedimiento sp_retirar_facultad (migración v3_002): comprueba que no
        // tenga hijos activos y lo retira en la MISMA transacción, para que
        // nadie le cuelgue un hijo entre la comprobación y el retiro.
        // Cero filas = no existe o ya estaba inactivo (404).
        try
        {
            return await conexion.ExecuteScalarAsync<int>(
                "dbo.sp_retirar_facultad", new { Id = id }, commandType: CommandType.StoredProcedure);
        }
        catch (SqlException ex) when (ex.Number == 50409)
        {
            throw new DependenciasActivasExcepcion(
                $"No se puede retirar la facultad {id}: {ex.Message}. Retírelos primero.");
        }
    }
}
