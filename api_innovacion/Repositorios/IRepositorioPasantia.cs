using ApiInnovacion.Modelos;

namespace ApiInnovacion.Repositorios;

/// <summary>
/// El contrato de la capa de datos para pasantia. El servicio conoce ESTA
/// interfaz y nada más.
/// </summary>
public interface IRepositorioPasantia
{
    Task<IEnumerable<Pasantia>> ObtenerTodos(int limite);
    Task<Pasantia?> ObtenerPorId(int id);
    Task Crear(Pasantia entidad);
    Task<int> Reemplazar(Pasantia entidad);
    Task<int> ActualizarParcial(int id, PasantiaCambios cambios);
    Task<int> EliminarLogico(int id);
}
