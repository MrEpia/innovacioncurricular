using ApiInnovacion.Modelos;

namespace ApiInnovacion.Repositorios;

public interface IRepositorioUsuario
{
    Task<IEnumerable<Usuario>> ObtenerTodos(int limite);
    Task<Usuario?> ObtenerPorId(int id);

    /// <summary>Crea el usuario con la contraseña YA hasheada. Devuelve el id generado.</summary>
    Task<int> Crear(Usuario usuario, string hashClave);

    Task<int> Reemplazar(Usuario usuario);
    Task<int> ActualizarParcial(int id, UsuarioCambios cambios, string? nuevoHash);
    Task<int> EliminarLogico(int id);

    /// <summary>Para el ingreso: el hash y si está activo. Busca también los retirados.</summary>
    Task<CredencialesUsuario?> ObtenerCredenciales(string username);

    /// <summary>Los nombres de los roles ACTIVOS asignados (asignación activa).</summary>
    Task<List<string>> RolesActivos(int usuarioId);

    /// <summary>Cuántos usuarios activos tienen hoy el rol administrador activo.</summary>
    Task<int> ContarAdministradoresActivos();

    /// <summary>Solo para el administrador inicial: reactiva al usuario y le pone la clave del .env.</summary>
    Task RestablecerAdministrador(int id, string hashClave, string email);
}
