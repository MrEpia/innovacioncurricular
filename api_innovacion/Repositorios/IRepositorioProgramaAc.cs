using ApiInnovacion.Modelos;

namespace ApiInnovacion.Repositorios;

/// <summary>
/// El contrato de la capa de datos para programa_ac. El servicio conoce ESTA
/// interfaz y nada más.
/// </summary>
public interface IRepositorioProgramaAc
{
    Task<IEnumerable<ProgramaAc>> ObtenerTodos(int limite);
    Task<ProgramaAc?> ObtenerPorLlave(int programa, string areaConocimiento);
    Task Crear(ProgramaAc entidad);
    Task<int> EliminarLogico(int programa, string areaConocimiento);
}
