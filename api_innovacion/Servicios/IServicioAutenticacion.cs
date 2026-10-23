namespace ApiInnovacion.Servicios;

/// <summary>Lo que devuelve un ingreso correcto.</summary>
public record ResultadoIngreso(
    string Token, DateTime ExpiraUtc, int UsuarioId, string Username, string? NombreCompleto, List<string> Roles);

/// <summary>El ingreso: verifica credenciales y emite el JWT.</summary>
public interface IServicioAutenticacion
{
    /// <summary>
    /// CredencialesInvalidasExcepcion (401) si el usuario no existe, está retirado
    /// o la clave no coincide · OperacionNoPermitidaExcepcion (403) si las
    /// credenciales son correctas pero no tiene ningún rol activo.
    /// </summary>
    Task<ResultadoIngreso> IniciarSesion(string username, string clave);
}
