using System.Data;
using ApiInnovacion.Excepciones;
using ApiInnovacion.Modelos;
using Dapper;
using Microsoft.Data.SqlClient;

namespace ApiInnovacion.Repositorios;

/// <summary>
/// La capa 3 contra SQL Server, con Dapper, para premio. El SQL se escribe
/// a mano, queda a la vista y SIEMPRE va parametrizado (@parametro).
///
/// Todas las consultas filtran por activo = 1: el borrado es lógico.
/// </summary>
public class RepositorioPremioSqlServer : IRepositorioPremio
{
    private readonly string _cadenaConexion;

    public RepositorioPremioSqlServer(IConfiguration configuracion)
    {
        _cadenaConexion = configuracion.GetConnectionString("SqlServer")
            ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'SqlServer'.");
    }

    private IDbConnection CrearConexion() => new SqlConnection(_cadenaConexion);

    // Los alias traducen snake_case (tabla) a PascalCase (entidad): Dapper
    // mapea por nombre.
    private const string COLUMNAS = @"id AS Id, nombre AS Nombre, descripcion AS Descripcion, fecha AS Fecha, entidad_otorgante AS EntidadOtorgante, pais AS Pais, programa AS Programa";

    public async Task<IEnumerable<Premio>> ObtenerTodos(int limite)
    {
        using var conexion = CrearConexion();
        var sql = $@"
            SELECT TOP (@Limite) {COLUMNAS}
            FROM premio
            WHERE activo = 1
            ORDER BY id ASC";

        return await conexion.QueryAsync<Premio>(sql, new { Limite = limite });
    }

    public async Task<Premio?> ObtenerPorId(int id)
    {
        using var conexion = CrearConexion();
        // Un registro inactivo responde como inexistente.
        var sql = $@"
            SELECT {COLUMNAS}
            FROM premio
            WHERE id = @Id AND activo = 1";

        return await conexion.QueryFirstOrDefaultAsync<Premio>(sql, new { Id = id });
    }

    public async Task Crear(Premio entidad)
    {
        using var conexion = CrearConexion();
        const string sql = @"
            INSERT INTO premio (id, nombre, descripcion, fecha, entidad_otorgante, pais, programa, activo)
            VALUES (@Id, @Nombre, @Descripcion, @Fecha, @EntidadOtorgante, @Pais, @Programa, 1)";

        try
        {
            await conexion.ExecuteAsync(sql, entidad);
        }
        catch (SqlException ex) when (ex.Number is 2627 or 2601)
        {
            // 2627/2601: violación de llave primaria o índice único. Se traduce
            // aquí para que el servicio nunca vea un tipo de SqlClient.
            throw new LlaveDuplicadaExcepcion(
                "Ya existe un registro de premio con esa llave (puede estar retirado).");
        }
    }

    public async Task<int> Reemplazar(Premio entidad)
    {
        using var conexion = CrearConexion();
        const string sql = @"
            UPDATE premio
            SET nombre = @Nombre, descripcion = @Descripcion, fecha = @Fecha, entidad_otorgante = @EntidadOtorgante, pais = @Pais, programa = @Programa
            WHERE id = @Id AND activo = 1";

        return await conexion.ExecuteAsync(sql, entidad);
    }

    public async Task<int> ActualizarParcial(int id, PremioCambios cambios)
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
        if (cambios.Descripcion != null)
        {
            asignaciones.Add("descripcion = @Descripcion");
            parametros.Add("Descripcion", cambios.Descripcion);
        }
        if (cambios.Fecha != null)
        {
            asignaciones.Add("fecha = @Fecha");
            parametros.Add("Fecha", cambios.Fecha);
        }
        if (cambios.EntidadOtorgante != null)
        {
            asignaciones.Add("entidad_otorgante = @EntidadOtorgante");
            parametros.Add("EntidadOtorgante", cambios.EntidadOtorgante);
        }
        if (cambios.Pais != null)
        {
            asignaciones.Add("pais = @Pais");
            parametros.Add("Pais", cambios.Pais);
        }
        if (cambios.Programa != null)
        {
            asignaciones.Add("programa = @Programa");
            parametros.Add("Programa", cambios.Programa);
        }

        if (asignaciones.Count == 0) return 0;

        var sql = $"UPDATE premio SET {string.Join(", ", asignaciones)} WHERE id = @Id AND activo = 1";

        return await conexion.ExecuteAsync(sql, parametros);
    }

    public async Task<int> EliminarLogico(int id)
    {
        using var conexion = CrearConexion();
        // Borrado LÓGICO en una sola consulta: cero filas = no existe o ya
        // estaba inactivo, que es el 404 del contrato.
        const string sql = @"
            UPDATE premio
            SET activo = 0
            WHERE id = @Id AND activo = 1";

        return await conexion.ExecuteAsync(sql, new { Id = id });
    }
}
