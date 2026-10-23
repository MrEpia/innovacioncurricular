using ApiInnovacion.Modelos;

namespace ApiInnovacion.Repositorios;

/// <summary>
/// El contrato de la capa de datos para enfoque_rc. El servicio conoce ESTA
/// interfaz y nada más.
/// </summary>
public interface IRepositorioEnfoqueRc
{
    Task<IEnumerable<EnfoqueRc>> ObtenerTodos(int limite);
    Task<EnfoqueRc?> ObtenerPorLlave(int enfoque, int registroCalificado);
    Task Crear(EnfoqueRc entidad);
    Task<int> EliminarLogico(int enfoque, int registroCalificado);
}
