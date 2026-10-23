using ApiInnovacion.Modelos;

namespace ApiInnovacion.Repositorios;

/// <summary>
/// El contrato de la capa de datos para registro_calificado. El servicio conoce ESTA
/// interfaz y nada más.
/// </summary>
public interface IRepositorioRegistroCalificado
{
    Task<IEnumerable<RegistroCalificado>> ObtenerTodos(int limite);
    Task<RegistroCalificado?> ObtenerPorCodigo(int codigo);
    Task Crear(RegistroCalificado entidad);
    Task<int> Reemplazar(RegistroCalificado entidad);
    Task<int> ActualizarParcial(int codigo, RegistroCalificadoCambios cambios);
    Task<int> EliminarLogico(int codigo);
}
