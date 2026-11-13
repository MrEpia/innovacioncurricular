// PostgreSQL: consultas explícitas; conserva el contrato del repositorio SQL Server.
using ApiInnovacion.Excepciones;
using System.Data;
using ApiInnovacion.Modelos;
using Dapper;
using Npgsql;

namespace ApiInnovacion.Repositorios.Postgres;

public class RepositorioPracticaEstrategiaPostgres : IRepositorioPracticaEstrategia
{
    private readonly string _cadenaConexion;

    public RepositorioPracticaEstrategiaPostgres(ConfiguracionPostgres configuracion)
    {
        _cadenaConexion = configuracion.CadenaConexion;
    }

    private IDbConnection CrearConexion() => new NpgsqlConnection(_cadenaConexion);

    private const string COLUMNAS = @"id AS Id, tipo AS Tipo, nombre AS Nombre, descripcion AS Descripcion";

    public async Task<IEnumerable<PracticaEstrategia>> ObtenerTodos(int limite)
    {
        using var conexion = CrearConexion();
        var sql = $@"
            SELECT {COLUMNAS}
            FROM practica_estrategia
            WHERE activo = 1
            ORDER BY id ASC LIMIT @Limite";

        return await conexion.QueryAsync<PracticaEstrategia>(sql, new { Limite = limite });
    }

    public async Task<PracticaEstrategia?> ObtenerPorId(int id)
    {
        using var conexion = CrearConexion();

        var sql = $@"
            SELECT {COLUMNAS}
            FROM practica_estrategia
            WHERE id = @Id AND activo = 1";

        return await conexion.QueryFirstOrDefaultAsync<PracticaEstrategia>(sql, new { Id = id });
    }

    public async Task Crear(PracticaEstrategia entidad)
    {
        using var conexion = CrearConexion();

        const string sql = @"
            INSERT INTO practica_estrategia (id, tipo, nombre, descripcion, activo)
            VALUES (@Id, @Tipo, @Nombre, @Descripcion, 1)";

        await conexion.ExecuteAsync(sql, entidad);
    }

    public async Task<int> Reemplazar(PracticaEstrategia entidad)
    {
        using var conexion = CrearConexion();
        const string sql = @"
            UPDATE practica_estrategia
            SET tipo = @Tipo, nombre = @Nombre, descripcion = @Descripcion
            WHERE id = @Id AND activo = 1";

        return await conexion.ExecuteAsync(sql, entidad);
    }

    public async Task<int> ActualizarParcial(int id, string? tipo, string? nombre, string? descripcion)
    {
        using var conexion = CrearConexion();

        var asignaciones = new List<string>();
        var parametros = new DynamicParameters();
        parametros.Add("Id", id);

        if (tipo != null)
        {
            asignaciones.Add("tipo = @Tipo");
            parametros.Add("Tipo", tipo);
        }
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

        var sql = $"UPDATE practica_estrategia SET {string.Join(", ", asignaciones)} WHERE id = @Id AND activo = 1";

        return await conexion.ExecuteAsync(sql, parametros);
    }

    public async Task<int> EliminarLogico(int id)
    {
        using var conexion = CrearConexion();

        try
        {
            return await conexion.ExecuteScalarAsync<int>(
                "SELECT * FROM public.sp_retirar_practica_estrategia(@Id)", new { Id = id });
        }
        catch (PostgresException ex) when (ex.SqlState == "P0409")
        {
            throw new DependenciasActivasExcepcion(
                $"No se puede retirar la práctica o estrategia {id}: {ex.MessageText}. Retírelos primero.");
        }
    }
}
