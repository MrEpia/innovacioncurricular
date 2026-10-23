using ApiInnovacion.Modelos;

namespace ApiInnovacion.Repositorios;

/// <summary>
/// El contrato de la capa de datos para premio. El servicio conoce ESTA
/// interfaz y nada más.
/// </summary>
public interface IRepositorioPremio
{
    Task<IEnumerable<Premio>> ObtenerTodos(int limite);
    Task<Premio?> ObtenerPorId(int id);
    Task Crear(Premio entidad);
    Task<int> Reemplazar(Premio entidad);
    Task<int> ActualizarParcial(int id, PremioCambios cambios);
    Task<int> EliminarLogico(int id);
}
