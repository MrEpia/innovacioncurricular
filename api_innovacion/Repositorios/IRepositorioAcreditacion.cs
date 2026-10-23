using ApiInnovacion.Modelos;

namespace ApiInnovacion.Repositorios;

/// <summary>
/// El contrato de la capa de datos para acreditacion. El servicio conoce ESTA
/// interfaz y nada más.
/// </summary>
public interface IRepositorioAcreditacion
{
    Task<IEnumerable<Acreditacion>> ObtenerTodos(int limite);
    Task<Acreditacion?> ObtenerPorResolucion(int resolucion);
    Task Crear(Acreditacion entidad);
    Task<int> Reemplazar(Acreditacion entidad);
    Task<int> ActualizarParcial(int resolucion, AcreditacionCambios cambios);
    Task<int> EliminarLogico(int resolucion);
}
