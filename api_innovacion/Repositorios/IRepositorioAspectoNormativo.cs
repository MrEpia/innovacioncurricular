using ApiInnovacion.Modelos;

namespace ApiInnovacion.Repositorios;

/// <summary>
/// El contrato de la capa de datos para aspecto_normativo. El servicio conoce ESTA
/// interfaz y nada más.
/// </summary>
public interface IRepositorioAspectoNormativo
{
    Task<IEnumerable<AspectoNormativo>> ObtenerTodos(int limite);
    Task<AspectoNormativo?> ObtenerPorId(int id);
    Task Crear(AspectoNormativo entidad);
    Task<int> Reemplazar(AspectoNormativo entidad);
    Task<int> ActualizarParcial(int id, string? tipo, string? descripcion, string? fuente);
    Task<int> EliminarLogico(int id);
}
