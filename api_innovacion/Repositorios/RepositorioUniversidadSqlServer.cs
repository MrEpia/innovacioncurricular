using ApiInnovacion.Excepciones;
using System.Data;
using ApiInnovacion.Modelos;
using Dapper;
using Microsoft.Data.SqlClient;

namespace ApiInnovacion.Repositorios;

/// <summary>
/// La capa 3 contra SQL Server, con Dapper, para universidad. El SQL se escribe a
/// mano, queda a la vista y SIEMPRE va parametrizado (@parametro).
///
/// Todas las consultas filtran por activo = 1: el borrado es lógico.
/// </summary>
public class RepositorioUniversidadSqlServer : IRepositorioUniversidad
{
    private readonly string _cadenaConexion;

    public RepositorioUniversidadSqlServer(IConfiguration configuracion)
    {
        _cadenaConexion = configuracion.GetConnectionString("SqlServer")
            ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'SqlServer'.");
    }

    private IDbConnection CrearConexion() => new SqlConnection(_cadenaConexion);

    // Los alias traducen los nombres de la tabla (snake_case) a los de la
    // entidad (PascalCase): Dapper mapea por nombre.
    private const string COLUMNAS = @"id AS Id, nombre AS Nombre, tipo AS Tipo, ciudad AS Ciudad";

    public async Task<IEnumerable<Universidad>> ObtenerTodos(int limite)
    {
        using var conexion = CrearConexion();
        var sql = $@"
            SELECT TOP (@Limite) {COLUMNAS}
            FROM universidad
            WHERE activo = 1
            ORDER BY id ASC";

        return await conexion.QueryAsync<Universidad>(sql, new { Limite = limite });
    }

    public async Task<Universidad?> ObtenerPorId(int id)
    {
        using var conexion = CrearConexion();
        // Un registro inactivo responde como inexistente.
        var sql = $@"
            SELECT {COLUMNAS}
            FROM universidad
            WHERE id = @Id AND activo = 1";

        return await conexion.QueryFirstOrDefaultAsync<Universidad>(sql, new { Id = id });
    }

    public async Task Crear(Universidad entidad)
    {
        using var conexion = CrearConexion();
        // Nace activo. Una llave repetida la rechaza la llave primaria: 500.
        const string sql = @"
            INSERT INTO universidad (id, nombre, tipo, ciudad, activo)
            VALUES (@Id, @Nombre, @Tipo, @Ciudad, 1)";

        await conexion.ExecuteAsync(sql, entidad);
    }

    public async Task<int> Reemplazar(Universidad entidad)
    {
        using var conexion = CrearConexion();
        const string sql = @"
            UPDATE universidad
            SET nombre = @Nombre, tipo = @Tipo, ciudad = @Ciudad
            WHERE id = @Id AND activo = 1";

        return await conexion.ExecuteAsync(sql, entidad);
    }

    public async Task<int> ActualizarParcial(int id, string? nombre, string? tipo, string? ciudad)
    {
        using var conexion = CrearConexion();

        // El PATCH escribe solo lo que llegó, así que la consulta se compone.
        // OJO: lo que se compone son NOMBRES DE COLUMNA de una lista cerrada,
        // escrita aquí; los VALORES siempre viajan como @parametro.
        var asignaciones = new List<string>();
        var parametros = new DynamicParameters();
        parametros.Add("Id", id);

        if (nombre != null)
        {
            asignaciones.Add("nombre = @Nombre");
            parametros.Add("Nombre", nombre);
        }
        if (tipo != null)
        {
            asignaciones.Add("tipo = @Tipo");
            parametros.Add("Tipo", tipo);
        }
        if (ciudad != null)
        {
            asignaciones.Add("ciudad = @Ciudad");
            parametros.Add("Ciudad", ciudad);
        }

        if (asignaciones.Count == 0) return 0;

        var sql = $"UPDATE universidad SET {string.Join(", ", asignaciones)} WHERE id = @Id AND activo = 1";

        return await conexion.ExecuteAsync(sql, parametros);
    }

    public async Task<int> EliminarLogico(int id)
    {
        using var conexion = CrearConexion();
        // Procedimiento sp_retirar_universidad (migración v3_002): comprueba que no
        // tenga hijos activos y lo retira en la MISMA transacción, para que
        // nadie le cuelgue un hijo entre la comprobación y el retiro.
        // Cero filas = no existe o ya estaba inactivo (404).
        try
        {
            return await conexion.ExecuteScalarAsync<int>(
                "dbo.sp_retirar_universidad", new { Id = id }, commandType: CommandType.StoredProcedure);
        }
        catch (SqlException ex) when (ex.Number == 50409)
        {
            throw new DependenciasActivasExcepcion(
                $"No se puede retirar la universidad {id}: {ex.Message}. Retírelos primero.");
        }
    }
}
