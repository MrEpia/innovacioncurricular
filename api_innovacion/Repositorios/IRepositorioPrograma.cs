using ApiInnovacion.Modelos;

namespace ApiInnovacion.Repositorios;

/// <summary>
/// El contrato de la capa de datos para programa. El servicio conoce ESTA
/// interfaz y nada más.
/// </summary>
public interface IRepositorioPrograma
{
    Task<IEnumerable<Programa>> ObtenerTodos(int limite);
    Task<Programa?> ObtenerPorId(int id);
    Task Crear(Programa entidad);
    Task<int> Reemplazar(Programa entidad);
    Task<int> ActualizarParcial(int id, ProgramaCambios cambios);
    Task<int> EliminarLogico(int id);
}
