using System.Data;
using ApiInnovacion.Modelos;
using Dapper;
using Microsoft.Data.SqlClient;

namespace ApiInnovacion.Repositorios;

/// <summary>
/// La capa 3 contra SQL Server, con Dapper, para area_conocimiento. El SQL se escribe a
/// mano, queda a la vista y SIEMPRE va parametrizado (@parametro).
///
/// Todas las consultas filtran por activo = 1: el borrado es lógico.
/// </summary>
public class RepositorioAreaConocimientoSqlServer : IRepositorioAreaConocimiento
{
    private readonly string _cadenaConexion;

    public RepositorioAreaConocimientoSqlServer(IConfiguration configuracion)
    {
        _cadenaConexion = configuracion.GetConnectionString("SqlServer")
            ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'SqlServer'.");
    }

    private IDbConnection CrearConexion() => new SqlConnection(_cadenaConexion);

    // Los alias traducen los nombres de la tabla (snake_case) a los de la
    // entidad (PascalCase): Dapper mapea por nombre.
    private const string COLUMNAS = @"id AS Id, gran_area AS GranArea, area AS Area, disciplina AS Disciplina";

    public async Task<IEnumerable<AreaConocimiento>> ObtenerTodos(int limite)
    {
        using var conexion = CrearConexion();
        var sql = $@"
            SELECT TOP (@Limite) {COLUMNAS}
            FROM area_conocimiento
            WHERE activo = 1
            ORDER BY id ASC";

        return await conexion.QueryAsync<AreaConocimiento>(sql, new { Limite = limite });
    }

    public async Task<AreaConocimiento?> ObtenerPorId(string id)
    {
        using var conexion = CrearConexion();
        // Un registro inactivo responde como inexistente.
        var sql = $@"
            SELECT {COLUMNAS}
            FROM area_conocimiento
            WHERE id = @Id AND activo = 1";

        return await conexion.QueryFirstOrDefaultAsync<AreaConocimiento>(sql, new { Id = id });
    }

    public async Task Crear(AreaConocimiento entidad)
    {
        using var conexion = CrearConexion();
        // Nace activo. Una llave repetida la rechaza la llave primaria: 500.
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

        // El PATCH escribe solo lo que llegó, así que la consulta se compone.
        // OJO: lo que se compone son NOMBRES DE COLUMNA de una lista cerrada,
        // escrita aquí; los VALORES siempre viajan como @parametro.
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
        // Borrado LÓGICO en una sola consulta: cero filas afectadas significa
        // "no existe o ya estaba inactivo", que es justo el 404 que pide el
        // contrato — sin necesidad de consultar antes.
        const string sql = @"
            UPDATE area_conocimiento
            SET activo = 0
            WHERE id = @Id AND activo = 1";

        return await conexion.ExecuteAsync(sql, new { Id = id });
    }
}
