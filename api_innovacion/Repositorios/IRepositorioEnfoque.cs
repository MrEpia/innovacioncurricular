using ApiInnovacion.Modelos;

namespace ApiInnovacion.Repositorios;

/// <summary>
/// El contrato de la capa de datos para enfoque. El servicio conoce ESTA
/// interfaz y nada más.
/// </summary>
public interface IRepositorioEnfoque
{
    Task<IEnumerable<Enfoque>> ObtenerTodos(int limite);
    Task<Enfoque?> ObtenerPorId(int id);
    Task Crear(Enfoque entidad);
    Task<int> Reemplazar(Enfoque entidad);
    Task<int> ActualizarParcial(int id, string? nombre, string? descripcion);
    Task<int> EliminarLogico(int id);
}
