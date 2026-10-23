using ApiInnovacion.Excepciones;
using ApiInnovacion.Modelos;
using ApiInnovacion.Repositorios;
using ApiInnovacion.Seguridad;

namespace ApiInnovacion.Servicios;

public class ServicioUsuario : IServicioUsuario
{
    private readonly IRepositorioUsuario _repositorio;
    private readonly IRepositorioRol _roles;
    private readonly IRepositorioRolUsuario _asignaciones;
    private readonly IHasheadorClaves _hasheador;

    public ServicioUsuario(IRepositorioUsuario repositorio, IRepositorioRol roles,
                           IRepositorioRolUsuario asignaciones, IHasheadorClaves hasheador)
    {
        _repositorio = repositorio;
        _roles = roles;
        _asignaciones = asignaciones;
        _hasheador = hasheador;
    }

    public async Task<IEnumerable<Usuario>> ObtenerTodos(int limite)
    {
        if (limite <= 0)
        {
            throw new ArgumentException("El parámetro limite debe ser un número mayor a 0.");
        }

        return await _repositorio.ObtenerTodos(limite);
    }

    public async Task<Usuario> ObtenerPorId(int id) =>
        await _repositorio.ObtenerPorId(id)
        ?? throw new NoEncontradoExcepcion($"No existe un usuario activo con id {id}.");

    public async Task<int> Crear(Usuario usuario, string clave)
    {
        // La contraseña NUNCA llega al repositorio en claro: se hashea aquí.
        return await _repositorio.Crear(usuario, _hasheador.Hashear(clave));
    }

    public async Task<int> Reemplazar(int id, Usuario usuario)
    {
        usuario.Id = id;
        var filas = await _repositorio.Reemplazar(usuario);
        if (filas == 0)
        {
            throw new NoEncontradoExcepcion($"No existe un usuario activo con id {id}.");
        }

        return filas;
    }

    public async Task<int> ActualizarParcial(int id, UsuarioCambios cambios)
    {
        if (!cambios.HayAlgo())
        {
            throw new ArgumentException("No se envió ningún campo para actualizar.");
        }

        var hash = cambios.Clave != null ? _hasheador.Hashear(cambios.Clave) : null;
        var filas = await _repositorio.ActualizarParcial(id, cambios, hash);
        if (filas == 0)
        {
            throw new NoEncontradoExcepcion($"No existe un usuario activo con id {id}.");
        }

        return filas;
    }

    public async Task<int> Eliminar(int id)
    {
        var usuario = await ObtenerPorId(id);

        // Retirar al último administrador dejaría el sistema sin nadie que pueda
        // administrar usuarios: no hay vuelta atrás desde la pantalla.
        if (usuario.Roles.Contains(Roles.Administrador) && await _repositorio.ContarAdministradoresActivos() <= 1)
        {
            throw new OperacionNoPermitidaExcepcion(
                "No se puede retirar al único administrador activo. Asigne el rol administrador a otro usuario primero.");
        }

        return await _repositorio.EliminarLogico(id);
    }

    public async Task AsegurarAdministradorInicial(string username, string email, string clave)
    {
        if (await _repositorio.ContarAdministradoresActivos() > 0)
        {
            return;
        }

        var rolAdmin = await _roles.ObtenerPorNombre(Roles.Administrador)
            ?? throw new InvalidOperationException(
                "No existe el rol 'administrador'. ¿Corrió la migración db/migraciones/v3_001_seguridad.sql?");

        var hash = _hasheador.Hashear(clave);
        var existente = await _repositorio.ObtenerCredenciales(username);
        int id;
        if (existente == null)
        {
            id = await _repositorio.Crear(new Usuario
            {
                Username = username,
                Email = email,
                NombreCompleto = "Administrador del sistema"
            }, hash);
        }
        else
        {
            id = existente.Id;
            await _repositorio.RestablecerAdministrador(id, hash, email);
        }

        await _asignaciones.Asignar(id, rolAdmin.Id);
    }
}
