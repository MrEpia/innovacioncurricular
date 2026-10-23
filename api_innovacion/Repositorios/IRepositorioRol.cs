using ApiInnovacion.Modelos;

namespace ApiInnovacion.Repositorios;

public interface IRepositorioRol
{
    Task<IEnumerable<Rol>> ObtenerTodos(int limite);
    Task<Rol?> ObtenerPorId(int id);
    Task<Rol?> ObtenerPorNombre(string nombre);
    Task<int> Crear(Rol rol);
    Task<int> Reemplazar(Rol rol);
    Task<int> ActualizarParcial(int id, RolCambios cambios);
    Task<int> EliminarLogico(int id);

    /// <summary>Cuántos usuarios activos tienen asignado (y activo) este rol.</summary>
    Task<int> UsuariosActivosConRol(int id);
}
