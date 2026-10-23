using ApiInnovacion.Modelos;

namespace ApiInnovacion.Repositorios;

/// <summary>
/// El contrato de la capa de datos para activ_academica. El servicio conoce ESTA
/// interfaz y nada más.
/// </summary>
public interface IRepositorioActivAcademica
{
    Task<IEnumerable<ActivAcademica>> ObtenerTodos(int limite);
    Task<ActivAcademica?> ObtenerPorId(int id);
    Task Crear(ActivAcademica entidad);
    Task<int> Reemplazar(ActivAcademica entidad);
    Task<int> ActualizarParcial(int id, ActivAcademicaCambios cambios);
    Task<int> EliminarLogico(int id);
}
