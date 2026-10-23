using ApiInnovacion.Modelos;

namespace ApiInnovacion.Repositorios;

/// <summary>
/// El contrato de la capa de datos para alianza. El servicio conoce ESTA
/// interfaz y nada más.
/// </summary>
public interface IRepositorioAlianza
{
    Task<IEnumerable<Alianza>> ObtenerTodos(int limite);
    Task<Alianza?> ObtenerPorLlave(int aliado, int departamento);
    Task Crear(Alianza entidad);
    Task<int> Reemplazar(Alianza entidad);
    Task<int> ActualizarParcial(int aliado, int departamento, AlianzaCambios cambios);
    Task<int> EliminarLogico(int aliado, int departamento);
}
