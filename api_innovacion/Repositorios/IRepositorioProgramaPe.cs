using ApiInnovacion.Modelos;

namespace ApiInnovacion.Repositorios;

/// <summary>
/// El contrato de la capa de datos para programa_pe. El servicio conoce ESTA
/// interfaz y nada más.
/// </summary>
public interface IRepositorioProgramaPe
{
    Task<IEnumerable<ProgramaPe>> ObtenerTodos(int limite);
    Task<ProgramaPe?> ObtenerPorLlave(int programa, int practicaEstrategia);
    Task Crear(ProgramaPe entidad);
    Task<int> EliminarLogico(int programa, int practicaEstrategia);
}
