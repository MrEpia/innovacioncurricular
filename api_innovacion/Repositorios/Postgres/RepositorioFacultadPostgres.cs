// PostgreSQL: consultas explícitas; conserva el contrato del repositorio SQL Server.
using System.Data;
using ApiInnovacion.Excepciones;
using ApiInnovacion.Modelos;
using Dapper;
using Npgsql;

namespace ApiInnovacion.Repositorios.Postgres;

public class RepositorioFacultadPostgres : IRepositorioFacultad
{
    private readonly string _cadenaConexion;

    public RepositorioFacultadPostgres(ConfiguracionPostgres configuracion)
    {
        _cadenaConexion = configuracion.CadenaConexion;
    }

    private IDbConnection CrearConexion() => new NpgsqlConnection(_cadenaConexion);

    private const string COLUMNAS = @"id AS Id, nombre AS Nombre, tipo AS Tipo, fecha_fun::timestamp AS FechaFun, universidad AS Universidad";

    public async Task<IEnumerable<Facultad>> ObtenerTodos(int limite)
    {
        using var conexion = CrearConexion();
        var sql = $@"
            SELECT {COLUMNAS}
            FROM facultad
            WHERE activo = 1
            ORDER BY id ASC LIMIT @Limite";

        return await conexion.QueryAsync<Facultad>(sql, new { Limite = limite });
    }

    public async Task<Facultad?> ObtenerPorId(int id)
    {
        using var conexion = CrearConexion();

        var sql = $@"
            SELECT {COLUMNAS}
            FROM facultad
            WHERE id = @Id AND activo = 1";

        return await conexion.QueryFirstOrDefaultAsync<Facultad>(sql, new { Id = id });
    }

    public async Task Crear(Facultad entidad)
    {
        using var conexion = CrearConexion();
        const string sql = @"
            INSERT INTO facultad (id, nombre, tipo, fecha_fun, universidad, activo)
            VALUES (@Id, @Nombre, @Tipo, @FechaFun, @Universidad, 1)";

        try
        {
            await conexion.ExecuteAsync(sql, entidad);
        }
        catch (PostgresException ex) when (ex.SqlState == "23505")
        {

            throw new LlaveDuplicadaExcepcion(
                "Ya existe un registro de facultad con esa llave (puede estar retirado).");
        }
    }

    public async Task<int> Reemplazar(Facultad entidad)
    {
        using var conexion = CrearConexion();
        const string sql = @"
            UPDATE facultad
            SET nombre = @Nombre, tipo = @Tipo, fecha_fun = @FechaFun, universidad = @Universidad
            WHERE id = @Id AND activo = 1";

        return await conexion.ExecuteAsync(sql, entidad);
    }

    public async Task<int> ActualizarParcial(int id, FacultadCambios cambios)
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
        if (cambios.Tipo != null)
        {
            asignaciones.Add("tipo = @Tipo");
            parametros.Add("Tipo", cambios.Tipo);
        }
        if (cambios.FechaFun != null)
        {
            asignaciones.Add("fecha_fun = @FechaFun");
            parametros.Add("FechaFun", cambios.FechaFun);
        }
        if (cambios.Universidad != null)
        {
            asignaciones.Add("universidad = @Universidad");
            parametros.Add("Universidad", cambios.Universidad);
        }

        if (asignaciones.Count == 0) return 0;

        var sql = $"UPDATE facultad SET {string.Join(", ", asignaciones)} WHERE id = @Id AND activo = 1";

        return await conexion.ExecuteAsync(sql, parametros);
    }

    public async Task<int> EliminarLogico(int id)
    {
        using var conexion = CrearConexion();

        try
        {
            return await conexion.ExecuteScalarAsync<int>(
                "SELECT * FROM public.sp_retirar_facultad(@Id)", new { Id = id });
        }
        catch (PostgresException ex) when (ex.SqlState == "P0409")
        {
            throw new DependenciasActivasExcepcion(
                $"No se puede retirar la facultad {id}: {ex.MessageText}. Retírelos primero.");
        }
    }
}
