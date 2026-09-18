using System.Data;
using ApiInnovacion.Modelos;
using Dapper;
using Microsoft.Data.SqlClient;

namespace ApiInnovacion.Repositorios;

/// <summary>
/// La capa 3 contra SQL Server, con Dapper, para practica_estrategia. El SQL se escribe a
/// mano, queda a la vista y SIEMPRE va parametrizado (@parametro).
///
/// Todas las consultas filtran por activo = 1: el borrado es lógico.
/// </summary>
public class RepositorioPracticaEstrategiaSqlServer : IRepositorioPracticaEstrategia
{
    private readonly string _cadenaConexion;

    public RepositorioPracticaEstrategiaSqlServer(IConfiguration configuracion)
    {
        _cadenaConexion = configuracion.GetConnectionString("SqlServer")
            ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'SqlServer'.");
    }

    private IDbConnection CrearConexion() => new SqlConnection(_cadenaConexion);

    // Los alias traducen los nombres de la tabla (snake_case) a los de la
    // entidad (PascalCase): Dapper mapea por nombre.
    private const string COLUMNAS = @"id AS Id, tipo AS Tipo, nombre AS Nombre, descripcion AS Descripcion";

    public async Task<IEnumerable<PracticaEstrategia>> ObtenerTodos(int limite)
    {
        using var conexion = CrearConexion();
        var sql = $@"
            SELECT TOP (@Limite) {COLUMNAS}
            FROM practica_estrategia
            WHERE activo = 1
            ORDER BY id ASC";

        return await conexion.QueryAsync<PracticaEstrategia>(sql, new { Limite = limite });
    }

    public async Task<PracticaEstrategia?> ObtenerPorId(int id)
    {
        using var conexion = CrearConexion();
        // Un registro inactivo responde como inexistente.
        var sql = $@"
            SELECT {COLUMNAS}
            FROM practica_estrategia
            WHERE id = @Id AND activo = 1";

        return await conexion.QueryFirstOrDefaultAsync<PracticaEstrategia>(sql, new { Id = id });
    }

    public async Task Crear(PracticaEstrategia entidad)
    {
        using var conexion = CrearConexion();
        // Nace activo. Una llave repetida la rechaza la llave primaria: 500.
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

        // El PATCH escribe solo lo que llegó, así que la consulta se compone.
        // OJO: lo que se compone son NOMBRES DE COLUMNA de una lista cerrada,
        // escrita aquí; los VALORES siempre viajan como @parametro.
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
        // Borrado LÓGICO en una sola consulta: cero filas afectadas significa
        // "no existe o ya estaba inactivo", que es justo el 404 que pide el
        // contrato — sin necesidad de consultar antes.
        const string sql = @"
            UPDATE practica_estrategia
            SET activo = 0
            WHERE id = @Id AND activo = 1";

        return await conexion.ExecuteAsync(sql, new { Id = id });
    }
}
