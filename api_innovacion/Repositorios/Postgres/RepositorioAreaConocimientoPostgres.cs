// PostgreSQL: consultas explícitas; conserva el contrato del repositorio SQL Server.
using ApiInnovacion.Excepciones;
using System.Data;
using ApiInnovacion.Modelos;
using Dapper;
using Npgsql;

namespace ApiInnovacion.Repositorios.Postgres;

public class RepositorioAreaConocimientoPostgres : IRepositorioAreaConocimiento
{
    private readonly string _cadenaConexion;

    public RepositorioAreaConocimientoPostgres(ConfiguracionPostgres configuracion)
    {
        _cadenaConexion = configuracion.CadenaConexion;
    }

    private IDbConnection CrearConexion() => new NpgsqlConnection(_cadenaConexion);

    private const string COLUMNAS = @"id AS Id, gran_area AS GranArea, area AS Area, disciplina AS Disciplina";

    public async Task<IEnumerable<AreaConocimiento>> ObtenerTodos(int limite)
    {
        using var conexion = CrearConexion();
        var sql = $@"
            SELECT {COLUMNAS}
            FROM area_conocimiento
            WHERE activo = 1
            ORDER BY id ASC LIMIT @Limite";

        return await conexion.QueryAsync<AreaConocimiento>(sql, new { Limite = limite });
    }

    public async Task<AreaConocimiento?> ObtenerPorId(string id)
    {
        using var conexion = CrearConexion();

        var sql = $@"
            SELECT {COLUMNAS}
            FROM area_conocimiento
            WHERE id = @Id AND activo = 1";

        return await conexion.QueryFirstOrDefaultAsync<AreaConocimiento>(sql, new { Id = id });
    }

    public async Task Crear(AreaConocimiento entidad)
    {
        using var conexion = CrearConexion();

        const string sql = @"
            INSERT INTO area_conocimiento (id, gran_area, area, disciplina, activo)
            VALUES (@Id, @GranArea, @Area, @Disciplina, 1)";

        await conexion.ExecuteAsync(sql, entidad);
    }

    public async Task<int> Reemplazar(AreaConocimiento entidad)
    {
        using var conexion = CrearConexion();
        const string sql = @"
            UPDATE area_conocimiento
            SET gran_area = @GranArea, area = @Area, disciplina = @Disciplina
            WHERE id = @Id AND activo = 1";

        return await conexion.ExecuteAsync(sql, entidad);
    }

    public async Task<int> ActualizarParcial(string id, string? granArea, string? area, string? disciplina)
    {
        using var conexion = CrearConexion();

        var asignaciones = new List<string>();
        var parametros = new DynamicParameters();
        parametros.Add("Id", id);

        if (granArea != null)
        {
            asignaciones.Add("gran_area = @GranArea");
            parametros.Add("GranArea", granArea);
        }
        if (area != null)
        {
            asignaciones.Add("area = @Area");
            parametros.Add("Area", area);
        }
        if (disciplina != null)
        {
            asignaciones.Add("disciplina = @Disciplina");
            parametros.Add("Disciplina", disciplina);
        }

        if (asignaciones.Count == 0) return 0;

        var sql = $"UPDATE area_conocimiento SET {string.Join(", ", asignaciones)} WHERE id = @Id AND activo = 1";

        return await conexion.ExecuteAsync(sql, parametros);
    }

    public async Task<int> EliminarLogico(string id)
    {
        using var conexion = CrearConexion();

        try
        {
            return await conexion.ExecuteScalarAsync<int>(
                "SELECT * FROM public.sp_retirar_area_conocimiento(@Id)", new { Id = id });
        }
        catch (PostgresException ex) when (ex.SqlState == "P0409")
        {
            throw new DependenciasActivasExcepcion(
                $"No se puede retirar el área de conocimiento {id}: {ex.MessageText}. Retírelos primero.");
        }
    }
}
