// PostgreSQL: consultas explícitas; conserva el contrato del repositorio SQL Server.
using System.Data;
using ApiInnovacion.Excepciones;
using ApiInnovacion.Modelos;
using Dapper;
using Npgsql;

namespace ApiInnovacion.Repositorios.Postgres;

public class RepositorioRolPostgres : IRepositorioRol
{
    private readonly string _cadenaConexion;

    public RepositorioRolPostgres(ConfiguracionPostgres configuracion)
    {
        _cadenaConexion = configuracion.CadenaConexion;
    }

    private IDbConnection CrearConexion() => new NpgsqlConnection(_cadenaConexion);

    private const string COLUMNAS = "id AS Id, nombre AS Nombre, descripcion AS Descripcion, fecha_creacion AS FechaCreacion";

    public async Task<IEnumerable<Rol>> ObtenerTodos(int limite)
    {
        using var conexion = CrearConexion();
        return await conexion.QueryAsync<Rol>($@"
            SELECT {COLUMNAS} FROM rol WHERE activo = 1 ORDER BY id LIMIT @Limite", new { Limite = limite });
    }

    public async Task<Rol?> ObtenerPorId(int id)
    {
        using var conexion = CrearConexion();
        return await conexion.QueryFirstOrDefaultAsync<Rol>($@"
            SELECT {COLUMNAS} FROM rol WHERE id = @Id AND activo = 1", new { Id = id });
    }

    public async Task<Rol?> ObtenerPorNombre(string nombre)
    {
        using var conexion = CrearConexion();
        return await conexion.QueryFirstOrDefaultAsync<Rol>($@"
            SELECT {COLUMNAS} FROM rol WHERE nombre = @Nombre AND activo = 1", new { Nombre = nombre });
    }

    public async Task<int> Crear(Rol rol)
    {
        using var conexion = CrearConexion();
        try
        {
            return await conexion.ExecuteScalarAsync<int>(@"
                INSERT INTO rol (nombre, descripcion, activo)
                
                VALUES (@Nombre, @Descripcion, 1) RETURNING id", rol);
        }
        catch (PostgresException ex) when (ex.SqlState == "23505")
        {
            throw new LlaveDuplicadaExcepcion($"Ya existe un rol llamado '{rol.Nombre}' (puede estar retirado).");
        }
    }

    public async Task<int> Reemplazar(Rol rol)
    {
        using var conexion = CrearConexion();
        try
        {
            return await conexion.ExecuteAsync(@"
                UPDATE rol SET nombre = @Nombre, descripcion = @Descripcion
                WHERE id = @Id AND activo = 1", rol);
        }
        catch (PostgresException ex) when (ex.SqlState == "23505")
        {
            throw new LlaveDuplicadaExcepcion($"Ya existe otro rol llamado '{rol.Nombre}'.");
        }
    }

    public async Task<int> ActualizarParcial(int id, RolCambios cambios)
    {
        using var conexion = CrearConexion();
        var asignaciones = new List<string>();
        var parametros = new DynamicParameters();
        parametros.Add("Id", id);

        if (cambios.Nombre != null) { asignaciones.Add("nombre = @Nombre"); parametros.Add("Nombre", cambios.Nombre); }
        if (cambios.Descripcion != null) { asignaciones.Add("descripcion = @Descripcion"); parametros.Add("Descripcion", cambios.Descripcion); }

        if (asignaciones.Count == 0) return 0;

        try
        {
            return await conexion.ExecuteAsync(
                $"UPDATE rol SET {string.Join(", ", asignaciones)} WHERE id = @Id AND activo = 1", parametros);
        }
        catch (PostgresException ex) when (ex.SqlState == "23505")
        {
            throw new LlaveDuplicadaExcepcion($"Ya existe otro rol llamado '{cambios.Nombre}'.");
        }
    }

    public async Task<int> EliminarLogico(int id)
    {
        using var conexion = CrearConexion();

        try
        {
            return await conexion.ExecuteScalarAsync<int>(
                "SELECT * FROM public.sp_rol_retirar(@Id)", new { Id = id });
        }
        catch (PostgresException ex) when (ex.SqlState == "P0409")
        {
            throw new DependenciasActivasExcepcion($"No se puede retirar el rol {id}: {ex.MessageText}. Quíteselo primero.");
        }
    }

    public async Task<int> UsuariosActivosConRol(int id)
    {
        using var conexion = CrearConexion();
        return await conexion.ExecuteScalarAsync<int>(@"
            SELECT COUNT(*)
            FROM rol_usuario ru
            JOIN usuario u ON u.id = ru.usuario_id AND u.activo = 1
            WHERE ru.rol_id = @Id AND ru.activo = 1", new { Id = id });
    }
}
