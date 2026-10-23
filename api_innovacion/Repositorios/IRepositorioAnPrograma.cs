using ApiInnovacion.Modelos;

namespace ApiInnovacion.Repositorios;

/// <summary>
/// El contrato de la capa de datos para an_programa. El servicio conoce ESTA
/// interfaz y nada más.
/// </summary>
public interface IRepositorioAnPrograma
{
    Task<IEnumerable<AnPrograma>> ObtenerTodos(int limite);
    Task<AnPrograma?> ObtenerPorLlave(int aspectoNormativo, int programa);
    Task Crear(AnPrograma entidad);
    Task<int> EliminarLogico(int aspectoNormativo, int programa);
}
