using ApiInnovacion.Modelos;

namespace ApiInnovacion.Repositorios;

/// <summary>
/// El contrato de la capa de datos para practica_estrategia. El servicio conoce ESTA
/// interfaz y nada más.
/// </summary>
public interface IRepositorioPracticaEstrategia
{
    Task<IEnumerable<PracticaEstrategia>> ObtenerTodos(int limite);
    Task<PracticaEstrategia?> ObtenerPorId(int id);
    Task Crear(PracticaEstrategia entidad);
    Task<int> Reemplazar(PracticaEstrategia entidad);
    Task<int> ActualizarParcial(int id, string? tipo, string? nombre, string? descripcion);
    Task<int> EliminarLogico(int id);
}
