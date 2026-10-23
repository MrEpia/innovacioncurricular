using ApiInnovacion.Modelos;

namespace ApiInnovacion.Servicios;

/// <summary>El contrato de negocio de rol (solo administradores, v3).</summary>
public interface IServicioRol
{
    Task<IEnumerable<Rol>> ObtenerTodos(int limite);
    Task<Rol> ObtenerPorId(int id);
    Task<int> Crear(Rol rol);
    Task<int> Reemplazar(int id, Rol rol);
    Task<int> ActualizarParcial(int id, RolCambios cambios);

    /// <summary>Borrado lógico. 409 si es uno de los tres roles del sistema o si tiene usuarios activos.</summary>
    Task<int> Eliminar(int id);
}
