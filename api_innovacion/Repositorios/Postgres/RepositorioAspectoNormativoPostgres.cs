// PostgreSQL: consultas explícitas; conserva el contrato del repositorio SQL Server.
using ApiInnovacion.Excepciones;
using System.Data;
using ApiInnovacion.Modelos;
using Dapper;
using Npgsql;

namespace ApiInnovacion.Repositorios.Postgres;

public class RepositorioAspectoNormativoPostgres : IRepositorioAspectoNormativo
{
    private readonly string _cadenaConexion;

    public RepositorioAspectoNormativoPostgres(ConfiguracionPostgres configuracion)
    {
        _cadenaConexion = configuracion.CadenaConexion;
    }

    private IDbConnection CrearConexion() => new NpgsqlConnection(_cadenaConexion);

    private const string COLUMNAS = @"id AS Id, tipo AS Tipo, descripcion AS Descripcion, fuente AS Fuente";

    public async Task<IEnumerable<AspectoNormativo>> ObtenerTodos(int limite)
    {
        using var conexion = CrearConexion();
        var sql = $@"
            SELECT {COLUMNAS}
            FROM aspecto_normativo
            WHERE activo = 1
            ORDER BY id ASC LIMIT @Limite";

        return await conexion.QueryAsync<AspectoNormativo>(sql, new { Limite = limite });
    }

    public async Task<AspectoNormativo?> ObtenerPorId(int id)
    {
        using var conexion = CrearConexion();

        var sql = $@"
            SELECT {COLUMNAS}
            FROM aspecto_normativo
            WHERE id = @Id AND activo = 1";

        return await conexion.QueryFirstOrDefaultAsync<AspectoNormativo>(sql, new { Id = id });
    }

    public async Task Crear(AspectoNormativo entidad)
    {
        using var conexion = CrearConexion();

        const string sql = @"
            INSERT INTO aspecto_normativo (id, tipo, descripcion, fuente, activo)
            VALUES (@Id, @Tipo, @Descripcion, @Fuente, 1)";

        await conexion.ExecuteAsync(sql, entidad);
    }

    public async Task<int> Reemplazar(AspectoNormativo entidad)
    {
        using var conexion = CrearConexion();
        const string sql = @"
            UPDATE aspecto_normativo
            SET tipo = @Tipo, descripcion = @Descripcion, fuente = @Fuente
            WHERE id = @Id AND activo = 1";

        return await conexion.ExecuteAsync(sql, entidad);
    }

    public async Task<int> ActualizarParcial(int id, string? tipo, string? descripcion, string? fuente)
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
        if (descripcion != null)
        {
            asignaciones.Add("descripcion = @Descripcion");
            parametros.Add("Descripcion", descripcion);
        }
        if (fuente != null)
        {
            asignaciones.Add("fuente = @Fuente");
            parametros.Add("Fuente", fuente);
        }

        if (asignaciones.Count == 0) return 0;

        var sql = $"UPDATE aspecto_normativo SET {string.Join(", ", asignaciones)} WHERE id = @Id AND activo = 1";

        return await conexion.ExecuteAsync(sql, parametros);
    }

    public async Task<int> EliminarLogico(int id)
    {
        using var conexion = CrearConexion();

        try
        {
            return await conexion.ExecuteScalarAsync<int>(
                "SELECT * FROM public.sp_retirar_aspecto_normativo(@Id)", new { Id = id });
        }
        catch (PostgresException ex) when (ex.SqlState == "P0409")
        {
            throw new DependenciasActivasExcepcion(
                $"No se puede retirar el aspecto normativo {id}: {ex.MessageText}. Retírelos primero.");
        }
    }
}
