using ApiInnovacion.Modelos;

namespace ApiInnovacion.Servicios;

/// <summary>El contrato de negocio de usuario (solo administradores, v3).</summary>
public interface IServicioUsuario
{
    Task<IEnumerable<Usuario>> ObtenerTodos(int limite);
    Task<Usuario> ObtenerPorId(int id);

    /// <summary>Crea el usuario con la clave hasheada. Devuelve el id generado. 409 si username o email se repiten.</summary>
    Task<int> Crear(Usuario usuario, string clave);

    Task<int> Reemplazar(int id, Usuario usuario);

    /// <summary>PATCH; si trae clave, se hashea. ArgumentException si no trae nada.</summary>
    Task<int> ActualizarParcial(int id, UsuarioCambios cambios);

    /// <summary>Borrado lógico. OperacionNoPermitida (409) si es el único administrador activo.</summary>
    Task<int> Eliminar(int id);

    /// <summary>Al arrancar: si no hay ningún administrador activo, crea (o restablece) el del .env.</summary>
    Task AsegurarAdministradorInicial(string username, string email, string clave);
}
