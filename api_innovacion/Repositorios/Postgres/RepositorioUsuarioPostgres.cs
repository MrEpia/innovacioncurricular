// PostgreSQL: consultas explícitas; conserva el contrato del repositorio SQL Server.
using System.Data;
using ApiInnovacion.Excepciones;
using ApiInnovacion.Modelos;
using Dapper;
using Npgsql;

namespace ApiInnovacion.Repositorios.Postgres;

public class RepositorioUsuarioPostgres : IRepositorioUsuario
{
    private readonly string _cadenaConexion;

    public RepositorioUsuarioPostgres(ConfiguracionPostgres configuracion)
    {
        _cadenaConexion = configuracion.CadenaConexion;
    }

    private IDbConnection CrearConexion() => new NpgsqlConnection(_cadenaConexion);

    private const string COLUMNAS = @"id AS Id, username AS Username, email AS Email,
        nombre_completo AS NombreCompleto, fecha_creacion AS FechaCreacion,
        fecha_actualizacion AS FechaActualizacion";

    public async Task<IEnumerable<Usuario>> ObtenerTodos(int limite)
    {
        using var conexion = CrearConexion();
        var usuarios = (await conexion.QueryAsync<Usuario>($@"
            SELECT {COLUMNAS}
            FROM usuario
            WHERE activo = 1
            ORDER BY id LIMIT @Limite", new { Limite = limite })).ToList();

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

        const string sql = @"
            INSERT INTO usuario (username, password, email, nombre_completo, activo)
            
            VALUES (@Username, @Password, @Email, @NombreCompleto, 1) RETURNING id";

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
        catch (PostgresException ex) when (ex.SqlState == "23505")
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
                fecha_actualizacion = LOCALTIMESTAMP
            WHERE id = @Id AND activo = 1";

        try
        {
            return await conexion.ExecuteAsync(sql, usuario);
        }
        catch (PostgresException ex) when (ex.SqlState == "23505")
        {
            throw new LlaveDuplicadaExcepcion("Ya existe otro usuario con ese username o ese email.");
        }
    }

    public async Task<int> ActualizarParcial(int id, UsuarioCambios cambios, string? nuevoHash)
    {
        using var conexion = CrearConexion();

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
        catch (PostgresException ex) when (ex.SqlState == "23505")
        {
            throw new LlaveDuplicadaExcepcion("Ya existe otro usuario con ese username o ese email.");
        }
    }

    public async Task<int> EliminarLogico(int id)
    {
        using var conexion = CrearConexion();

        try
        {
            return await conexion.ExecuteScalarAsync<int>(
                "SELECT * FROM public.sp_usuario_retirar(@Id)", new { Id = id });
        }
        catch (PostgresException ex) when (ex.SqlState == "P0410")
        {
            throw new OperacionNoPermitidaExcepcion(ex.MessageText);
        }
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
        return await conexion.ExecuteScalarAsync<int>(
            "SELECT * FROM public.sp_contar_administradores_activos()");
    }

    public async Task RestablecerAdministrador(int id, string hashClave, string email)
    {
        using var conexion = CrearConexion();
        await conexion.ExecuteAsync(@"
            UPDATE usuario
            SET password = @Password, email = @Email, activo = 1, fecha_actualizacion = LOCALTIMESTAMP
            WHERE id = @Id", new { Id = id, Password = hashClave, Email = email });
    }
}
