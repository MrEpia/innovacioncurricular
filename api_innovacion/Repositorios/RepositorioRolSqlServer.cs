using System.Data;
using ApiInnovacion.Excepciones;
using ApiInnovacion.Modelos;
using Dapper;
using Microsoft.Data.SqlClient;

namespace ApiInnovacion.Repositorios;

/// <summary>rol contra SQL Server. El id lo genera la base (IDENTITY).</summary>
public class RepositorioRolSqlServer : IRepositorioRol
{
    private readonly string _cadenaConexion;

    public RepositorioRolSqlServer(IConfiguration configuracion)
    {
        _cadenaConexion = configuracion.GetConnectionString("SqlServer")
            ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'SqlServer'.");
    }

    private IDbConnection CrearConexion() => new SqlConnection(_cadenaConexion);

    private const string COLUMNAS = "id AS Id, nombre AS Nombre, descripcion AS Descripcion, fecha_creacion AS FechaCreacion";

    public async Task<IEnumerable<Rol>> ObtenerTodos(int limite)
    {
        using var conexion = CrearConexion();
        return await conexion.QueryAsync<Rol>($@"
            SELECT TOP (@Limite) {COLUMNAS} FROM rol WHERE activo = 1 ORDER BY id", new { Limite = limite });
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
                OUTPUT INSERTED.id
                VALUES (@Nombre, @Descripcion, 1)", rol);
        }
        catch (SqlException ex) when (ex.Number is 2627 or 2601)
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
        catch (SqlException ex) when (ex.Number is 2627 or 2601)
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
        catch (SqlException ex) when (ex.Number is 2627 or 2601)
        {
            throw new LlaveDuplicadaExcepcion($"Ya existe otro rol llamado '{cambios.Nombre}'.");
        }
    }

    public async Task<int> EliminarLogico(int id)
    {
        using var conexion = CrearConexion();
        // sp_rol_retirar: comprueba que ningún usuario activo lo tenga y lo
        // retira en la misma transacción.
        try
        {
            return await conexion.ExecuteScalarAsync<int>(
                "dbo.sp_rol_retirar", new { Id = id }, commandType: CommandType.StoredProcedure);
        }
        catch (SqlException ex) when (ex.Number == 50409)
        {
            throw new DependenciasActivasExcepcion($"No se puede retirar el rol {id}: {ex.Message}. Quíteselo primero.");
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
