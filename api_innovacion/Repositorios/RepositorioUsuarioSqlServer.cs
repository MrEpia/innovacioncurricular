using System.Data;
using ApiInnovacion.Excepciones;
using ApiInnovacion.Modelos;
using Dapper;
using Microsoft.Data.SqlClient;

namespace ApiInnovacion.Repositorios;

/// <summary>
/// usuario contra SQL Server, con el SQL a la vista. La columna password solo
/// se lee en ObtenerCredenciales: ninguna otra consulta la selecciona, así que
/// no hay forma de que termine en una respuesta por descuido.
/// </summary>
public class RepositorioUsuarioSqlServer : IRepositorioUsuario
{
    private readonly string _cadenaConexion;

    public RepositorioUsuarioSqlServer(IConfiguration configuracion)
    {
        _cadenaConexion = configuracion.GetConnectionString("SqlServer")
            ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'SqlServer'.");
    }

    private IDbConnection CrearConexion() => new SqlConnection(_cadenaConexion);

    private const string COLUMNAS = @"id AS Id, username AS Username, email AS Email,
        nombre_completo AS NombreCompleto, fecha_creacion AS FechaCreacion,
        fecha_actualizacion AS FechaActualizacion";

    public async Task<IEnumerable<Usuario>> ObtenerTodos(int limite)
    {
        using var conexion = CrearConexion();
        var usuarios = (await conexion.QueryAsync<Usuario>($@"
            SELECT TOP (@Limite) {COLUMNAS}
            FROM usuario
            WHERE activo = 1
            ORDER BY id", new { Limite = limite })).ToList();

        foreach (var u in usuarios)
        {
            u.Roles = await RolesActivos(u.Id);
        }

        return usuarios;
    }

    public async Task<Usuario?> ObtenerPorId(int id)
    {
        using var conexion = CrearConexion();
        var usuario = await conexion.QueryFirstOrDefaultAsync<Usuario>($@"
            SELECT {COLUMNAS} FROM usuario WHERE id = @Id AND activo = 1", new { Id = id });

        if (usuario != null)
        {
            usuario.Roles = await RolesActivos(usuario.Id);
        }

        return usuario;
    }

    public async Task<int> Crear(Usuario usuario, string hashClave)
    {
        using var conexion = CrearConexion();
        // OUTPUT INSERTED.id devuelve el id que generó IDENTITY en la misma
        // consulta, sin una segunda ida a la base.
        const string sql = @"
            INSERT INTO usuario (username, password, email, nombre_completo, activo)
            OUTPUT INSERTED.id
            VALUES (@Username, @Password, @Email, @NombreCompleto, 1)";

        try
        {
            return await conexion.ExecuteScalarAsync<int>(sql, new
            {
                usuario.Username,
                Password = hashClave,
                usuario.Email,
                usuario.NombreCompleto
            });
        }
        catch (SqlException ex) when (ex.Number is 2627 or 2601)
        {
            throw new LlaveDuplicadaExcepcion("Ya existe un usuario con ese username o ese email.");
        }
    }

    public async Task<int> Reemplazar(Usuario usuario)
    {
        using var conexion = CrearConexion();
        const string sql = @"
            UPDATE usuario
            SET username = @Username, email = @Email, nombre_completo = @NombreCompleto,
                fecha_actualizacion = GETDATE()
            WHERE id = @Id AND activo = 1";

        try
        {
            return await conexion.ExecuteAsync(sql, usuario);
        }
        catch (SqlException ex) when (ex.Number is 2627 or 2601)
        {
            throw new LlaveDuplicadaExcepcion("Ya existe otro usuario con ese username o ese email.");
        }
    }

    public async Task<int> ActualizarParcial(int id, UsuarioCambios cambios, string? nuevoHash)
    {
        using var conexion = CrearConexion();

        // Igual que en la v1: se componen NOMBRES de columna de una lista
        // cerrada; los valores siempre como @parametro.
        var asignaciones = new List<string> { "fecha_actualizacion = GETDATE()" };
        var parametros = new DynamicParameters();
        parametros.Add("Id", id);

        if (cambios.Username != null) { asignaciones.Add("username = @Username"); parametros.Add("Username", cambios.Username); }
        if (cambios.Email != null) { asignaciones.Add("email = @Email"); parametros.Add("Email", cambios.Email); }
        if (cambios.NombreCompleto != null) { asignaciones.Add("nombre_completo = @NombreCompleto"); parametros.Add("NombreCompleto", cambios.NombreCompleto); }
        if (nuevoHash != null) { asignaciones.Add("password = @Password"); parametros.Add("Password", nuevoHash); }

        var sql = $"UPDATE usuario SET {string.Join(", ", asignaciones)} WHERE id = @Id AND activo = 1";

        try
        {
            return await conexion.ExecuteAsync(sql, parametros);
        }
        catch (SqlException ex) when (ex.Number is 2627 or 2601)
        {
            throw new LlaveDuplicadaExcepcion("Ya existe otro usuario con ese username o ese email.");
        }
    }

    public async Task<int> EliminarLogico(int id)
    {
        using var conexion = CrearConexion();
        return await conexion.ExecuteAsync(@"
            UPDATE usuario SET activo = 0, fecha_actualizacion = GETDATE()
            WHERE id = @Id AND activo = 1", new { Id = id });
    }

    public async Task<CredencialesUsuario?> ObtenerCredenciales(string username)
    {
        using var conexion = CrearConexion();
        return await conexion.QueryFirstOrDefaultAsync<CredencialesUsuario>(@"
            SELECT id AS Id, username AS Username, nombre_completo AS NombreCompleto,
                   password AS Password, activo AS Activo
            FROM usuario
            WHERE username = @Username", new { Username = username });
    }

    public async Task<List<string>> RolesActivos(int usuarioId)
    {
        using var conexion = CrearConexion();
        // Un rol cuenta solo si la ASIGNACIÓN está activa y el ROL también.
        var roles = await conexion.QueryAsync<string>(@"
            SELECT r.nombre
            FROM rol_usuario ru
            JOIN rol r ON r.id = ru.rol_id
            WHERE ru.usuario_id = @Id AND ru.activo = 1 AND r.activo = 1
            ORDER BY r.nombre", new { Id = usuarioId });
        return roles.ToList();
    }

    public async Task<int> ContarAdministradoresActivos()
    {
        using var conexion = CrearConexion();
        return await conexion.ExecuteScalarAsync<int>(@"
            SELECT COUNT(DISTINCT u.id)
            FROM usuario u
            JOIN rol_usuario ru ON ru.usuario_id = u.id AND ru.activo = 1
            JOIN rol r ON r.id = ru.rol_id AND r.activo = 1
            WHERE u.activo = 1 AND r.nombre = 'administrador'");
    }

    public async Task RestablecerAdministrador(int id, string hashClave, string email)
    {
        using var conexion = CrearConexion();
        await conexion.ExecuteAsync(@"
            UPDATE usuario
            SET password = @Password, email = @Email, activo = 1, fecha_actualizacion = GETDATE()
            WHERE id = @Id", new { Id = id, Password = hashClave, Email = email });
    }
}
