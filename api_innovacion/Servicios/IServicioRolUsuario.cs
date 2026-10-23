using ApiInnovacion.Modelos;

namespace ApiInnovacion.Servicios;

/// <summary>El contrato de negocio de rol_usuario: asignar y quitar roles (solo administradores).</summary>
public interface IServicioRolUsuario
{
    Task<IEnumerable<RolUsuario>> ObtenerTodos(int limite);
    Task<RolUsuario> ObtenerPorLlave(int usuarioId, int rolId);

    /// <summary>400 si el usuario o el rol no están activos · 409 si ya lo tenía.</summary>
    Task Asignar(int usuarioId, int rolId);

    /// <summary>Borrado lógico. 409 si es quitarle el rol administrador al último administrador.</summary>
    Task<int> Quitar(int usuarioId, int rolId);
}
