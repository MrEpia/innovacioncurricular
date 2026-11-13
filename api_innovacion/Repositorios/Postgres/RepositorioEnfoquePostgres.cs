// PostgreSQL: consultas explícitas; conserva el contrato del repositorio SQL Server.
using ApiInnovacion.Excepciones;
using System.Data;
using ApiInnovacion.Modelos;
using Dapper;
using Npgsql;

namespace ApiInnovacion.Repositorios.Postgres;

public class RepositorioEnfoquePostgres : IRepositorioEnfoque
{
    private readonly string _cadenaConexion;

    public RepositorioEnfoquePostgres(ConfiguracionPostgres configuracion)
    {
        _cadenaConexion = configuracion.CadenaConexion;
    }

    private IDbConnection CrearConexion() => new NpgsqlConnection(_cadenaConexion);

    private const string COLUMNAS = @"id AS Id, nombre AS Nombre, descripcion AS Descripcion";

    public async Task<IEnumerable<Enfoque>> ObtenerTodos(int limite)
    {
        using var conexion = CrearConexion();
        var sql = $@"
            SELECT {COLUMNAS}
            FROM enfoque
            WHERE activo = 1
            ORDER BY id ASC LIMIT @Limite";

        return await conexion.QueryAsync<Enfoque>(sql, new { Limite = limite });
    }

    public async Task<Enfoque?> ObtenerPorId(int id)
    {
        using var conexion = CrearConexion();

        var sql = $@"
            SELECT {COLUMNAS}
            FROM enfoque
            WHERE id = @Id AND activo = 1";

        return await conexion.QueryFirstOrDefaultAsync<Enfoque>(sql, new { Id = id });
    }

    public async Task Crear(Enfoque entidad)
    {
        using var conexion = CrearConexion();

        const string sql = @"
            INSERT INTO enfoque (id, nombre, descripcion, activo)
            VALUES (@Id, @Nombre, @Descripcion, 1)";

        await conexion.ExecuteAsync(sql, entidad);
    }

    public async Task<int> Reemplazar(Enfoque entidad)
    {
        using var conexion = CrearConexion();
        const string sql = @"
            UPDATE enfoque
            SET nombre = @Nombre, descripcion = @Descripcion
            WHERE id = @Id AND activo = 1";

        return await conexion.ExecuteAsync(sql, entidad);
    }

    public async Task<int> ActualizarParcial(int id, string? nombre, string? descripcion)
    {
        using var conexion = CrearConexion();

        var asignaciones = new List<string>();
        var parametros = new DynamicParameters();
        parametros.Add("Id", id);

        if (nombre != null)
        {
            asignaciones.Add("nombre = @Nombre");
            parametros.Add("Nombre", nombre);
        }
        if (descripcion != null)
        {
            asignaciones.Add("descripcion = @Descripcion");
            parametros.Add("Descripcion", descripcion);
        }

        if (asignaciones.Count == 0) return 0;

        var sql = $"UPDATE enfoque SET {string.Join(", ", asignaciones)} WHERE id = @Id AND activo = 1";

        return await conexion.ExecuteAsync(sql, parametros);
    }

    public async Task<int> EliminarLogico(int id)
    {
        using var conexion = CrearConexion();

        try
        {
            return await conexion.ExecuteScalarAsync<int>(
                "SELECT * FROM public.sp_retirar_enfoque(@Id)", new { Id = id });
        }
        catch (PostgresException ex) when (ex.SqlState == "P0409")
        {
            throw new DependenciasActivasExcepcion(
                $"No se puede retirar el enfoque {id}: {ex.MessageText}. Retírelos primero.");
        }
    }
}
