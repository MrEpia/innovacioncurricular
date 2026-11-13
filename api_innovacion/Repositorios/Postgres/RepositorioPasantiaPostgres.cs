// PostgreSQL: consultas explícitas; conserva el contrato del repositorio SQL Server.
using System.Data;
using ApiInnovacion.Excepciones;
using ApiInnovacion.Modelos;
using Dapper;
using Npgsql;

namespace ApiInnovacion.Repositorios.Postgres;

public class RepositorioPasantiaPostgres : IRepositorioPasantia
{
    private readonly string _cadenaConexion;

    public RepositorioPasantiaPostgres(ConfiguracionPostgres configuracion)
    {
        _cadenaConexion = configuracion.CadenaConexion;
    }

    private IDbConnection CrearConexion() => new NpgsqlConnection(_cadenaConexion);

    private const string COLUMNAS = @"id AS Id, nombre AS Nombre, pais AS Pais, empresa AS Empresa, descripcion AS Descripcion, programa AS Programa";

    public async Task<IEnumerable<Pasantia>> ObtenerTodos(int limite)
    {
        using var conexion = CrearConexion();
        var sql = $@"
            SELECT {COLUMNAS}
            FROM pasantia
            WHERE activo = 1
            ORDER BY id ASC LIMIT @Limite";

        return await conexion.QueryAsync<Pasantia>(sql, new { Limite = limite });
    }

    public async Task<Pasantia?> ObtenerPorId(int id)
    {
        using var conexion = CrearConexion();

        var sql = $@"
            SELECT {COLUMNAS}
            FROM pasantia
            WHERE id = @Id AND activo = 1";

        return await conexion.QueryFirstOrDefaultAsync<Pasantia>(sql, new { Id = id });
    }

    public async Task Crear(Pasantia entidad)
    {
        using var conexion = CrearConexion();
        const string sql = @"
            INSERT INTO pasantia (id, nombre, pais, empresa, descripcion, programa, activo)
            VALUES (@Id, @Nombre, @Pais, @Empresa, @Descripcion, @Programa, 1)";

        try
        {
            await conexion.ExecuteAsync(sql, entidad);
        }
        catch (PostgresException ex) when (ex.SqlState == "23505")
        {

            throw new LlaveDuplicadaExcepcion(
                "Ya existe un registro de pasantia con esa llave (puede estar retirado).");
        }
    }

    public async Task<int> Reemplazar(Pasantia entidad)
    {
        using var conexion = CrearConexion();
        const string sql = @"
            UPDATE pasantia
            SET nombre = @Nombre, pais = @Pais, empresa = @Empresa, descripcion = @Descripcion, programa = @Programa
            WHERE id = @Id AND activo = 1";

        return await conexion.ExecuteAsync(sql, entidad);
    }

    public async Task<int> ActualizarParcial(int id, PasantiaCambios cambios)
    {
        using var conexion = CrearConexion();

        var asignaciones = new List<string>();
        var parametros = new DynamicParameters();
        parametros.Add("Id", id);

        if (cambios.Nombre != null)
        {
            asignaciones.Add("nombre = @Nombre");
            parametros.Add("Nombre", cambios.Nombre);
        }
        if (cambios.Pais != null)
        {
            asignaciones.Add("pais = @Pais");
            parametros.Add("Pais", cambios.Pais);
        }
        if (cambios.Empresa != null)
        {
            asignaciones.Add("empresa = @Empresa");
            parametros.Add("Empresa", cambios.Empresa);
        }
        if (cambios.Descripcion != null)
        {
            asignaciones.Add("descripcion = @Descripcion");
            parametros.Add("Descripcion", cambios.Descripcion);
        }
        if (cambios.Programa != null)
        {
            asignaciones.Add("programa = @Programa");
            parametros.Add("Programa", cambios.Programa);
        }

        if (asignaciones.Count == 0) return 0;

        var sql = $"UPDATE pasantia SET {string.Join(", ", asignaciones)} WHERE id = @Id AND activo = 1";

        return await conexion.ExecuteAsync(sql, parametros);
    }

    public async Task<int> EliminarLogico(int id)
    {
        using var conexion = CrearConexion();

        const string sql = @"
            UPDATE pasantia
            SET activo = 0
            WHERE id = @Id AND activo = 1";

        return await conexion.ExecuteAsync(sql, new { Id = id });
    }
}
