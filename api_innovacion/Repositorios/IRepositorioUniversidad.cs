using ApiInnovacion.Modelos;

namespace ApiInnovacion.Repositorios;

/// <summary>
/// El contrato de la capa de datos para universidad. El servicio conoce ESTA
/// interfaz y nada más.
/// </summary>
public interface IRepositorioUniversidad
{
    Task<IEnumerable<Universidad>> ObtenerTodos(int limite);
    Task<Universidad?> ObtenerPorId(int id);
    Task Crear(Universidad entidad);
    Task<int> Reemplazar(Universidad entidad);
    Task<int> ActualizarParcial(int id, string? nombre, string? tipo, string? ciudad);
    Task<int> EliminarLogico(int id);
}
