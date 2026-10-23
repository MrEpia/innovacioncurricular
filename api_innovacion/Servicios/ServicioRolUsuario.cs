using ApiInnovacion.Excepciones;
using ApiInnovacion.Modelos;
using ApiInnovacion.Repositorios;
using ApiInnovacion.Seguridad;

namespace ApiInnovacion.Servicios;

public class ServicioRolUsuario : IServicioRolUsuario
{
    private readonly IRepositorioRolUsuario _repositorio;
    private readonly IRepositorioReferencias _referencias;
    private readonly IRepositorioUsuario _usuarios;

    public ServicioRolUsuario(IRepositorioRolUsuario repositorio, IRepositorioReferencias referencias,
                              IRepositorioUsuario usuarios)
    {
        _repositorio = repositorio;
        _referencias = referencias;
        _usuarios = usuarios;
    }

    public async Task<IEnumerable<RolUsuario>> ObtenerTodos(int limite)
    {
        if (limite <= 0)
        {
            throw new ArgumentException("El parámetro limite debe ser un número mayor a 0.");
        }

        return await _repositorio.ObtenerTodos(limite);
    }

    public async Task<RolUsuario> ObtenerPorLlave(int usuarioId, int rolId) =>
        await _repositorio.ObtenerPorLlave(usuarioId, rolId)
        ?? throw new NoEncontradoExcepcion($"El usuario {usuarioId} no tiene asignado el rol {rolId}.");

    public async Task Asignar(int usuarioId, int rolId)
    {
        if (!await _referencias.UsuarioActivo(usuarioId))
        {
            throw new ReferenciaInvalidaExcepcion($"El usuario {usuarioId} no existe o está retirado.");
        }

        if (!await _referencias.RolActivo(rolId))
        {
            throw new ReferenciaInvalidaExcepcion($"El rol {rolId} no existe o está retirado.");
        }

        if (!await _repositorio.Asignar(usuarioId, rolId))
        {
            throw new LlaveDuplicadaExcepcion($"El usuario {usuarioId} ya tiene el rol {rolId}.");
        }
    }

    public async Task<int> Quitar(int usuarioId, int rolId)
    {
        var asignacion = await ObtenerPorLlave(usuarioId, rolId);

        if (asignacion.Rol == Roles.Administrador && await _usuarios.ContarAdministradoresActivos() <= 1)
        {
            throw new OperacionNoPermitidaExcepcion(
                "No se puede quitar el rol administrador al único administrador activo.");
        }

        return await _repositorio.EliminarLogico(usuarioId, rolId);
    }
}
