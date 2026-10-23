using ApiInnovacion.Modelos;

namespace ApiInnovacion.Repositorios;

public interface IRepositorioRolUsuario
{
    Task<IEnumerable<RolUsuario>> ObtenerTodos(int limite);
    Task<RolUsuario?> ObtenerPorLlave(int usuarioId, int rolId);

    /// <summary>
    /// Asigna el rol. Devuelve true si quedó asignado (nuevo o reactivado) y
    /// false si ya estaba activo.
    /// </summary>
    Task<bool> Asignar(int usuarioId, int rolId);

    Task<int> EliminarLogico(int usuarioId, int rolId);
}
