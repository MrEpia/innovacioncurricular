// PostgreSQL: consultas explícitas; conserva el contrato del repositorio SQL Server.
using ApiInnovacion.Excepciones;
using System.Data;
using ApiInnovacion.Modelos;
using Dapper;
using Npgsql;

namespace ApiInnovacion.Repositorios.Postgres;

public class RepositorioCarInnovacionPostgres : IRepositorioCarInnovacion
{
    private readonly string _cadenaConexion;

    public RepositorioCarInnovacionPostgres(ConfiguracionPostgres configuracion)
    {
        _cadenaConexion = configuracion.CadenaConexion;
    }

    private IDbConnection CrearConexion() => new NpgsqlConnection(_cadenaConexion);

    private const string COLUMNAS = @"id AS Id, nombre AS Nombre, descripcion AS Descripcion, tipo AS Tipo";

    public async Task<IEnumerable<CarInnovacion>> ObtenerTodos(int limite)
    {
        using var conexion = CrearConexion();
        var sql = $@"
            SELECT {COLUMNAS}
            FROM car_innovacion
            WHERE activo = 1
            ORDER BY id ASC LIMIT @Limite";

        return await conexion.QueryAsync<CarInnovacion>(sql, new { Limite = limite });
    }

    public async Task<CarInnovacion?> ObtenerPorId(int id)
    {
        using var conexion = CrearConexion();

        var sql = $@"
            SELECT {COLUMNAS}
            FROM car_innovacion
            WHERE id = @Id AND activo = 1";

        return await conexion.QueryFirstOrDefaultAsync<CarInnovacion>(sql, new { Id = id });
    }

    public async Task Crear(CarInnovacion entidad)
    {
        using var conexion = CrearConexion();

        const string sql = @"
            INSERT INTO car_innovacion (id, nombre, descripcion, tipo, activo)
            VALUES (@Id, @Nombre, @Descripcion, @Tipo, 1)";

        await conexion.ExecuteAsync(sql, entidad);
    }

    public async Task<int> Reemplazar(CarInnovacion entidad)
    {
        using var conexion = CrearConexion();
        const string sql = @"
            UPDATE car_innovacion
            SET nombre = @Nombre, descripcion = @Descripcion, tipo = @Tipo
            WHERE id = @Id AND activo = 1";

        return await conexion.ExecuteAsync(sql, entidad);
    }

    public async Task<int> ActualizarParcial(int id, string? nombre, string? descripcion, string? tipo)
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
        if (tipo != null)
        {
            asignaciones.Add("tipo = @Tipo");
            parametros.Add("Tipo", tipo);
        }

        if (asignaciones.Count == 0) return 0;

        var sql = $"UPDATE car_innovacion SET {string.Join(", ", asignaciones)} WHERE id = @Id AND activo = 1";

        return await conexion.ExecuteAsync(sql, parametros);
    }

    public async Task<int> EliminarLogico(int id)
    {
        using var conexion = CrearConexion();

        try
        {
            return await conexion.ExecuteScalarAsync<int>(
                "SELECT * FROM public.sp_retirar_car_innovacion(@Id)", new { Id = id });
        }
        catch (PostgresException ex) when (ex.SqlState == "P0409")
        {
            throw new DependenciasActivasExcepcion(
                $"No se puede retirar la característica de innovación {id}: {ex.MessageText}. Retírelos primero.");
        }
    }
}
