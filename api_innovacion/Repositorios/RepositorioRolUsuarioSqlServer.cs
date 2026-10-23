using System.Data;
using ApiInnovacion.Modelos;
using Dapper;
using Microsoft.Data.SqlClient;

namespace ApiInnovacion.Repositorios;

/// <summary>
/// rol_usuario contra SQL Server. La tabla del curso no traía columna activo:
/// la agrega la migración v3_001 para que quitar un rol también sea un borrado
/// LÓGICO (Artículo 6).
/// </summary>
public class RepositorioRolUsuarioSqlServer : IRepositorioRolUsuario
{
    private readonly string _cadenaConexion;

    public RepositorioRolUsuarioSqlServer(IConfiguration configuracion)
    {
        _cadenaConexion = configuracion.GetConnectionString("SqlServer")
            ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'SqlServer'.");
    }

    private IDbConnection CrearConexion() => new SqlConnection(_cadenaConexion);

    private const string SELECCION = @"
        SELECT ru.usuario_id AS UsuarioId, ru.rol_id AS RolId, u.username AS Username, r.nombre AS Rol
        FROM rol_usuario ru
        JOIN usuario u ON u.id = ru.usuario_id
        JOIN rol r ON r.id = ru.rol_id";

    public async Task<IEnumerable<RolUsuario>> ObtenerTodos(int limite)
    {
        using var conexion = CrearConexion();
        return await conexion.QueryAsync<RolUsuario>($@"
            SELECT TOP (@Limite) * FROM ({SELECCION} WHERE ru.activo = 1) AS a
            ORDER BY a.UsuarioId, a.RolId", new { Limite = limite });
    }

    public async Task<RolUsuario?> ObtenerPorLlave(int usuarioId, int rolId)
    {
        using var conexion = CrearConexion();
        return await conexion.QueryFirstOrDefaultAsync<RolUsuario>($@"
            {SELECCION}
            WHERE ru.usuario_id = @UsuarioId AND ru.rol_id = @RolId AND ru.activo = 1",
            new { UsuarioId = usuarioId, RolId = rolId });
    }

    public async Task<bool> Asignar(int usuarioId, int rolId)
    {
        using var conexion = CrearConexion();
        // Tres casos en una sola consulta:
        //   - ya está activa        → no hace nada y responde 0 (el servicio dirá 409)
        //   - existe pero retirada  → la reactiva: la llave es la misma
        //   - no existe             → la crea
        const string sql = @"
            IF EXISTS (SELECT 1 FROM rol_usuario WHERE usuario_id = @UsuarioId AND rol_id = @RolId AND activo = 1)
                SELECT 0;
            ELSE IF EXISTS (SELECT 1 FROM rol_usuario WHERE usuario_id = @UsuarioId AND rol_id = @RolId)
            BEGIN
                UPDATE rol_usuario SET activo = 1 WHERE usuario_id = @UsuarioId AND rol_id = @RolId;
                SELECT 1;
            END
            ELSE
            BEGIN
                INSERT INTO rol_usuario (usuario_id, rol_id, activo) VALUES (@UsuarioId, @RolId, 1);
                SELECT 1;
            END";

        return await conexion.ExecuteScalarAsync<int>(sql, new { UsuarioId = usuarioId, RolId = rolId }) == 1;
    }

    public async Task<int> EliminarLogico(int usuarioId, int rolId)
    {
        using var conexion = CrearConexion();
        return await conexion.ExecuteAsync(@"
            UPDATE rol_usuario SET activo = 0
            WHERE usuario_id = @UsuarioId AND rol_id = @RolId AND activo = 1",
            new { UsuarioId = usuarioId, RolId = rolId });
    }
}
