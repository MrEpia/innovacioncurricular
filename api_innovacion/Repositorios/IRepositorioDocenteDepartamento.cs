using ApiInnovacion.Modelos;

namespace ApiInnovacion.Repositorios;

/// <summary>
/// El contrato de la capa de datos para docente_departamento. El servicio conoce ESTA
/// interfaz y nada más.
/// </summary>
public interface IRepositorioDocenteDepartamento
{
    Task<IEnumerable<DocenteDepartamento>> ObtenerTodos(int limite);
    Task<DocenteDepartamento?> ObtenerPorLlave(int docente, int departamento);
    Task Crear(DocenteDepartamento entidad);
    Task<int> Reemplazar(DocenteDepartamento entidad);
    Task<int> ActualizarParcial(int docente, int departamento, DocenteDepartamentoCambios cambios);
    Task<int> EliminarLogico(int docente, int departamento);
}
