using ApiInnovacion.Excepciones;
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
        // sp_rol_usuario_asignar, en una transacción: 0 si ya estaba activa
        // (el servicio dirá 409), 1 si la creó o la reactivó.
        return await conexion.ExecuteScalarAsync<int>(
            "dbo.sp_rol_usuario_asignar", new { UsuarioId = usuarioId, RolId = rolId },
            commandType: CommandType.StoredProcedure) == 1;
    }

    public async Task<int> EliminarLogico(int usuarioId, int rolId)
    {
        using var conexion = CrearConexion();
        // sp_rol_usuario_quitar: no deja quitar el rol administrador al último
        // administrador activo, ni siquiera con dos peticiones simultáneas.
        try
        {
            return await conexion.ExecuteScalarAsync<int>(
                "dbo.sp_rol_usuario_quitar", new { UsuarioId = usuarioId, RolId = rolId },
                commandType: CommandType.StoredProcedure);
        }
        catch (SqlException ex) when (ex.Number == 50410)
        {
            throw new OperacionNoPermitidaExcepcion(ex.Message);
        }
    }
}
