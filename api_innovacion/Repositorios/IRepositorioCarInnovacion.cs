using ApiInnovacion.Modelos;

namespace ApiInnovacion.Repositorios;

/// <summary>
/// El contrato de la capa de datos para car_innovacion. El servicio conoce ESTA
/// interfaz y nada más.
/// </summary>
public interface IRepositorioCarInnovacion
{
    Task<IEnumerable<CarInnovacion>> ObtenerTodos(int limite);
    Task<CarInnovacion?> ObtenerPorId(int id);
    Task Crear(CarInnovacion entidad);
    Task<int> Reemplazar(CarInnovacion entidad);
    Task<int> ActualizarParcial(int id, string? nombre, string? descripcion, string? tipo);
    Task<int> EliminarLogico(int id);
}
