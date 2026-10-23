using ApiInnovacion.Excepciones;
using ApiInnovacion.Repositorios;
using ApiInnovacion.Seguridad;

namespace ApiInnovacion.Servicios;

public class ServicioAutenticacion : IServicioAutenticacion
{
    private readonly IRepositorioUsuario _usuarios;
    private readonly IHasheadorClaves _hasheador;
    private readonly IGeneradorToken _generador;

    public ServicioAutenticacion(IRepositorioUsuario usuarios, IHasheadorClaves hasheador, IGeneradorToken generador)
    {
        _usuarios = usuarios;
        _hasheador = hasheador;
        _generador = generador;
    }

    public async Task<ResultadoIngreso> IniciarSesion(string username, string clave)
    {
        var credenciales = await _usuarios.ObtenerCredenciales(username.Trim());

        // Usuario inexistente, retirado o clave equivocada: los tres responden
        // igual (CredencialesInvalidasExcepcion explica por qué).
        if (credenciales == null || !credenciales.Activo || !_hasheador.Verificar(clave, credenciales.Password))
        {
            throw new CredencialesInvalidasExcepcion();
        }

        var roles = await _usuarios.RolesActivos(credenciales.Id);
        if (roles.Count == 0)
        {
            // Aquí sí se puede decir la causa: quien llegó hasta este punto ya
            // demostró conocer la contraseña.
            throw new OperacionNoPermitidaExcepcion(
                "El usuario no tiene ningún rol activo. Pídale al administrador que le asigne uno.");
        }

        var token = _generador.Generar(credenciales.Id, credenciales.Username, credenciales.NombreCompleto, roles);
        return new ResultadoIngreso(token.Token, token.ExpiraUtc, credenciales.Id, credenciales.Username,
                                    credenciales.NombreCompleto, roles);
    }
}
