using ApiInnovacion.Modelos;

namespace ApiInnovacion.Repositorios;

/// <summary>
/// El contrato de la capa de datos para facultad. El servicio conoce ESTA
/// interfaz y nada más.
/// </summary>
public interface IRepositorioFacultad
{
    Task<IEnumerable<Facultad>> ObtenerTodos(int limite);
    Task<Facultad?> ObtenerPorId(int id);
    Task Crear(Facultad entidad);
    Task<int> Reemplazar(Facultad entidad);
    Task<int> ActualizarParcial(int id, FacultadCambios cambios);
    Task<int> EliminarLogico(int id);
}
