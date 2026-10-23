using ApiInnovacion.Modelos;

namespace ApiInnovacion.Repositorios;

/// <summary>
/// El contrato de la capa de datos para programa_ci. El servicio conoce ESTA
/// interfaz y nada más.
/// </summary>
public interface IRepositorioProgramaCi
{
    Task<IEnumerable<ProgramaCi>> ObtenerTodos(int limite);
    Task<ProgramaCi?> ObtenerPorLlave(int programa, int carInnovacion);
    Task Crear(ProgramaCi entidad);
    Task<int> EliminarLogico(int programa, int carInnovacion);
}
