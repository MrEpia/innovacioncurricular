// PostgreSQL: consultas explícitas; conserva el contrato del repositorio SQL Server.
using ApiInnovacion.Excepciones;
using System.Data;
using ApiInnovacion.Modelos;
using Dapper;
using Npgsql;

namespace ApiInnovacion.Repositorios.Postgres;

public class RepositorioRolUsuarioPostgres : IRepositorioRolUsuario
{
    private readonly string _cadenaConexion;

    public RepositorioRolUsuarioPostgres(ConfiguracionPostgres configuracion)
    {
        _cadenaConexion = configuracion.CadenaConexion;
    }

    private IDbConnection CrearConexion() => new NpgsqlConnection(_cadenaConexion);

    private const string SELECCION = @"
        SELECT ru.usuario_id AS UsuarioId, ru.rol_id AS RolId, u.username AS Username, r.nombre AS Rol
        FROM rol_usuario ru
        JOIN usuario u ON u.id = ru.usuario_id
        JOIN rol r ON r.id = ru.rol_id";

    public async Task<IEnumerable<RolUsuario>> ObtenerTodos(int limite)
    {
        using var conexion = CrearConexion();
        return await conexion.QueryAsync<RolUsuario>($@"
            SELECT * FROM ({SELECCION} WHERE ru.activo = 1) AS a
            ORDER BY a.UsuarioId, a.RolId LIMIT @Limite", new { Limite = limite });
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

        return await conexion.ExecuteScalarAsync<int>(
            "SELECT * FROM public.sp_rol_usuario_asignar(@UsuarioId, @RolId)", new { UsuarioId = usuarioId, RolId = rolId }) == 1;
    }

    public async Task<int> EliminarLogico(int usuarioId, int rolId)
    {
        using var conexion = CrearConexion();

        try
        {
            return await conexion.ExecuteScalarAsync<int>(
                "SELECT * FROM public.sp_rol_usuario_quitar(@UsuarioId, @RolId)", new { UsuarioId = usuarioId, RolId = rolId });
        }
        catch (PostgresException ex) when (ex.SqlState == "P0410")
        {
            throw new OperacionNoPermitidaExcepcion(ex.MessageText);
        }
    }
}
