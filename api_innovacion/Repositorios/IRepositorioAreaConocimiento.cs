using ApiInnovacion.Modelos;

namespace ApiInnovacion.Repositorios;

/// <summary>
/// El contrato de la capa de datos para area_conocimiento. El servicio conoce ESTA
/// interfaz y nada más.
/// </summary>
public interface IRepositorioAreaConocimiento
{
    Task<IEnumerable<AreaConocimiento>> ObtenerTodos(int limite);
    Task<AreaConocimiento?> ObtenerPorId(string id);
    Task Crear(AreaConocimiento entidad);
    Task<int> Reemplazar(AreaConocimiento entidad);
    Task<int> ActualizarParcial(string id, string? granArea, string? area, string? disciplina);
    Task<int> EliminarLogico(string id);
}
