using ApiInnovacion.Excepciones;
using ApiInnovacion.Modelos;
using ApiInnovacion.Repositorios;
using ApiInnovacion.Seguridad;

namespace ApiInnovacion.Servicios;

public class ServicioRol : IServicioRol
{
    // Los permisos del sistema están escritos con estos tres nombres
    // (PermisoPorVerbo): retirarlos o renombrarlos dejaría reglas apuntando a
    // nada.
    private static readonly string[] RolesDelSistema = { Roles.Administrador, Roles.Editor, Roles.Consulta };

    private readonly IRepositorioRol _repositorio;

    public ServicioRol(IRepositorioRol repositorio)
    {
        _repositorio = repositorio;
    }

    public async Task<IEnumerable<Rol>> ObtenerTodos(int limite)
    {
        if (limite <= 0)
        {
            throw new ArgumentException("El parámetro limite debe ser un número mayor a 0.");
        }

        return await _repositorio.ObtenerTodos(limite);
    }

    public async Task<Rol> ObtenerPorId(int id) =>
        await _repositorio.ObtenerPorId(id)
        ?? throw new NoEncontradoExcepcion($"No existe un rol activo con id {id}.");

    public async Task<int> Crear(Rol rol) => await _repositorio.Crear(rol);

    public async Task<int> Reemplazar(int id, Rol rol)
    {
        await NoEsDelSistemaSiCambiaElNombre(id, rol.Nombre);
        rol.Id = id;
        var filas = await _repositorio.Reemplazar(rol);
        return filas == 0 ? throw new NoEncontradoExcepcion($"No existe un rol activo con id {id}.") : filas;
    }

    public async Task<int> ActualizarParcial(int id, RolCambios cambios)
    {
        if (!cambios.HayAlgo())
        {
            throw new ArgumentException("No se envió ningún campo para actualizar.");
        }

        if (cambios.Nombre != null)
        {
            await NoEsDelSistemaSiCambiaElNombre(id, cambios.Nombre);
        }

        var filas = await _repositorio.ActualizarParcial(id, cambios);
        return filas == 0 ? throw new NoEncontradoExcepcion($"No existe un rol activo con id {id}.") : filas;
    }

    public async Task<int> Eliminar(int id)
    {
        var rol = await ObtenerPorId(id);

        if (RolesDelSistema.Contains(rol.Nombre))
        {
            throw new OperacionNoPermitidaExcepcion($"El rol '{rol.Nombre}' es del sistema y no se puede retirar.");
        }

        var usuarios = await _repositorio.UsuariosActivosConRol(id);
        if (usuarios > 0)
        {
            throw new DependenciasActivasExcepcion(
                $"No se puede retirar el rol '{rol.Nombre}': lo tienen {usuarios} usuario(s) activo(s). Quíteselo primero.");
        }

        return await _repositorio.EliminarLogico(id);
    }

    private async Task NoEsDelSistemaSiCambiaElNombre(int id, string nuevoNombre)
    {
        var actual = await ObtenerPorId(id);
        if (RolesDelSistema.Contains(actual.Nombre) && actual.Nombre != nuevoNombre)
        {
            throw new OperacionNoPermitidaExcepcion($"El rol '{actual.Nombre}' es del sistema y no se puede renombrar.");
        }
    }
}
