using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ApiInnovacion.Excepciones;
using ApiInnovacion.Modelos;
using ApiInnovacion.Repositorios;
using ApiInnovacion.Seguridad;
using ApiInnovacion.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace ApiInnovacion.Pruebas;

/// <summary>
/// Prueba de capas de la v3 — Sebastián Calvo. El hash, el token, el ingreso,
/// los permisos por verbo y las reglas de usuarios y roles, SIN SQL Server.
/// </summary>
public static class PruebasSeguridad
{
    public static async Task Ejecutar()
    {
        Console.WriteLine();
        Console.WriteLine("--- v3 · seguridad ---");

        ProbarHash();
        ProbarToken();
        ProbarPermisosPorVerbo();
        await ProbarIngreso();
        await ProbarUsuariosYRoles();
    }

    private static void ProbarHash()
    {
        var h = new HasheadorClavesPbkdf2();
        var hash = h.Hashear("Clave_Segura_1");
        Verificador.Revisar(!hash.Contains("Clave_Segura_1") && hash.StartsWith("pbkdf2-sha256$"),
            "hash: la contraseña no aparece en lo que se guarda.", "hash: el texto guardado contiene la clave o no tiene el formato.");
        Verificador.Revisar(h.Verificar("Clave_Segura_1", hash) && !h.Verificar("otra", hash),
            "hash: verifica la clave correcta y rechaza una distinta.", "hash: la verificación falló.");
        Verificador.Revisar(h.Hashear("Clave_Segura_1") != hash,
            "hash: la misma clave da hashes distintos (sal aleatoria).", "hash: dos hashes iguales: no hay sal.");
        Verificador.Revisar(!h.Verificar("Clave_Segura_1", "Clave_Segura_1"),
            "hash: una contraseña guardada en texto plano NUNCA se acepta.", "hash: aceptó texto plano.");
    }

    private static GeneradorTokenJwt Generador(int minutos = 60) => new(Options.Create(new OpcionesJwt
    {
        Secreto = "secreto-de-prueba-de-al-menos-32-caracteres!",
        Minutos = minutos
    }));

    private static void ProbarToken()
    {
        var token = Generador().Generar(7, "ana", "Ana Pérez", new[] { Roles.Editor, Roles.Consulta });
        var leido = new JwtSecurityTokenHandler().ReadJwtToken(token.Token);
        var roles = leido.Claims.Where(c => c.Type == "role").Select(c => c.Value).ToList();
        Verificador.Revisar(roles.Contains(Roles.Editor) && roles.Contains(Roles.Consulta) && leido.Subject == "7",
            "token: lleva el id del usuario y sus roles.", $"token: claims inesperados ({string.Join(",", roles)}).");
        Verificador.Revisar(!token.Token.Contains("clave", StringComparison.OrdinalIgnoreCase),
            "token: no lleva la contraseña.", "token: contiene algo que parece la clave.");
        Verificador.Revisar(Math.Abs((token.ExpiraUtc - DateTime.UtcNow.AddMinutes(60)).TotalSeconds) < 5,
            "token: vence en los minutos configurados.", "token: vencimiento inesperado.");

        try
        {
            new OpcionesJwt { Secreto = "corto", Minutos = 60 }.Validar();
            Verificador.Revisar(false, "", "token: aceptó un secreto de menos de 32 caracteres.");
        }
        catch (InvalidOperationException)
        {
            Verificador.Revisar(true, "token: un secreto corto impide arrancar.", "");
        }
    }

    private static void ProbarPermisosPorVerbo()
    {
        bool Puede(string metodo, params string[] roles)
        {
            var http = new DefaultHttpContext();
            http.Request.Method = metodo;
            var manejador = new PermisoPorVerboManejador(new HttpContextAccessor { HttpContext = http });
            var usuario = new ClaimsPrincipal(new ClaimsIdentity(roles.Select(r => new Claim(ClaimTypes.Role, r)), "prueba"));
            var requisito = new PermisoPorVerboRequisito();
            var contexto = new AuthorizationHandlerContext(new[] { requisito }, usuario, null);
            manejador.HandleAsync(contexto).GetAwaiter().GetResult();
            return contexto.HasSucceeded;
        }

        Verificador.Revisar(Puede("GET", Roles.Consulta) && !Puede("POST", Roles.Consulta) && !Puede("DELETE", Roles.Consulta),
            "permisos: consulta solo lee.", "permisos: consulta puede más de lo debido.");
        Verificador.Revisar(Puede("GET", Roles.Editor) && Puede("POST", Roles.Editor) && Puede("PATCH", Roles.Editor)
                            && Puede("PUT", Roles.Editor) && !Puede("DELETE", Roles.Editor),
            "permisos: editor lee y modifica, pero no retira.", "permisos: editor con permisos equivocados.");
        Verificador.Revisar(Puede("DELETE", Roles.Administrador) && Puede("POST", Roles.Administrador),
            "permisos: administrador puede todo.", "permisos: administrador sin acceso completo.");
        Verificador.Revisar(!Puede("GET") && !Puede("OPTIONS", Roles.Administrador),
            "permisos: sin rol no se lee nada, y un verbo no previsto no se permite a nadie.",
            "permisos: se permitió algo no previsto.");
    }

    private static async Task ProbarIngreso()
    {
        var h = new HasheadorClavesPbkdf2();
        var usuarios = new RepositorioUsuarioFalso();
        usuarios.Agregar(1, "ana", h.Hashear("Clave_Ana_1"), activo: true, Roles.Editor);
        usuarios.Agregar(2, "beto", h.Hashear("Clave_Beto_1"), activo: false, Roles.Consulta);
        usuarios.Agregar(3, "caro", h.Hashear("Clave_Caro_1"), activo: true);
        var servicio = new ServicioAutenticacion(usuarios, h, Generador());

        var ingreso = await servicio.IniciarSesion("ana", "Clave_Ana_1");
        Verificador.Revisar(ingreso.Roles.SequenceEqual(new[] { Roles.Editor }) && ingreso.Token.Length > 20,
            "ingreso: credenciales correctas → token con sus roles.", "ingreso: no devolvió token o roles.");

        await Verificador.EsperarExcepcion<CredencialesInvalidasExcepcion>(
            () => servicio.IniciarSesion("ana", "equivocada"), "ingreso: clave equivocada → 401.");
        await Verificador.EsperarExcepcion<CredencialesInvalidasExcepcion>(
            () => servicio.IniciarSesion("nadie", "Clave_Ana_1"), "ingreso: usuario inexistente → 401 (mismo mensaje).");
        await Verificador.EsperarExcepcion<CredencialesInvalidasExcepcion>(
            () => servicio.IniciarSesion("beto", "Clave_Beto_1"), "ingreso: usuario retirado → 401.");
        await Verificador.EsperarExcepcion<OperacionNoPermitidaExcepcion>(
            () => servicio.IniciarSesion("caro", "Clave_Caro_1"), "ingreso: sin ningún rol activo → 403.");
    }

    private static async Task ProbarUsuariosYRoles()
    {
        var h = new HasheadorClavesPbkdf2();
        var usuarios = new RepositorioUsuarioFalso();
        usuarios.Agregar(1, "admin", h.Hashear("Clave_Admin_1"), activo: true, Roles.Administrador);
        var roles = new RepositorioRolFalso();
        var asignaciones = new RepositorioRolUsuarioFalso(usuarios, roles);
        var servicioUsuario = new ServicioUsuario(usuarios, roles, asignaciones, h);

        var id = await servicioUsuario.Crear(new Usuario { Username = "nuevo", Email = "n@x.co" }, "Clave_Nueva_1");
        Verificador.Revisar(usuarios.Hash(id) != "Clave_Nueva_1" && h.Verificar("Clave_Nueva_1", usuarios.Hash(id)),
            "usuario: se guarda el hash, no la clave.", "usuario: la clave quedó en texto plano.");

        await servicioUsuario.ActualizarParcial(id, new UsuarioCambios { Clave = "Clave_Cambiada_2" });
        Verificador.Revisar(h.Verificar("Clave_Cambiada_2", usuarios.Hash(id)),
            "usuario: cambiar la clave por PATCH la vuelve a hashear.", "usuario: el PATCH no cambió la clave.");

        await Verificador.EsperarExcepcion<OperacionNoPermitidaExcepcion>(
            () => servicioUsuario.Eliminar(1), "usuario: el único administrador activo no se puede retirar (409).");

        var servicioRol = new ServicioRol(roles);
        await Verificador.EsperarExcepcion<OperacionNoPermitidaExcepcion>(
            () => servicioRol.Eliminar(roles.IdDe(Roles.Administrador)), "rol: los roles del sistema no se retiran (409).");
        var auditor = await servicioRol.Crear(new Rol { Nombre = "auditor" });
        usuarios.AsignarRol(id, "auditor");
        await Verificador.EsperarExcepcion<DependenciasActivasExcepcion>(
            () => servicioRol.Eliminar(auditor), "rol: un rol con usuarios activos no se retira (409).");

        var referencias = new RepositorioReferenciasFalso();
        referencias.Usuarios.Add(id);
        referencias.RolesActivos.Add(roles.IdDe(Roles.Editor));
        var servicioAsignacion = new ServicioRolUsuario(asignaciones, referencias, usuarios);
        await Verificador.EsperarExcepcion<ReferenciaInvalidaExcepcion>(
            () => servicioAsignacion.Asignar(999, roles.IdDe(Roles.Editor)), "rol_usuario: usuario inexistente → 400.");
        await servicioAsignacion.Asignar(id, roles.IdDe(Roles.Editor));
        await Verificador.EsperarExcepcion<LlaveDuplicadaExcepcion>(
            () => servicioAsignacion.Asignar(id, roles.IdDe(Roles.Editor)), "rol_usuario: asignar dos veces el mismo rol → 409.");
        await Verificador.EsperarExcepcion<OperacionNoPermitidaExcepcion>(
            () => servicioAsignacion.Quitar(1, roles.IdDe(Roles.Administrador)),
            "rol_usuario: quitarle el rol administrador al único administrador → 409.");
    }
}

// ============================================================================
// Repositorios de mentiras de la v3.
// ============================================================================

public class RepositorioUsuarioFalso : IRepositorioUsuario
{
    private sealed class Fila
    {
        public Usuario Usuario = new();
        public string Hash = string.Empty;
        public bool Activo;
        public List<string> Roles = new();
    }

    private readonly Dictionary<int, Fila> _filas = new();

    public void Agregar(int id, string username, string hash, bool activo, params string[] roles) =>
        _filas[id] = new Fila { Usuario = new Usuario { Id = id, Username = username, Email = $"{username}@x.co" }, Hash = hash, Activo = activo, Roles = roles.ToList() };

    public string Hash(int id) => _filas[id].Hash;
    public void AsignarRol(int id, string rol) => _filas[id].Roles.Add(rol);
    public void QuitarRol(int id, string rol) => _filas[id].Roles.Remove(rol);
    public bool Activo(int id) => _filas.TryGetValue(id, out var f) && f.Activo;
    public List<string> RolesDe(int id) => _filas[id].Roles;

    public Task<IEnumerable<Usuario>> ObtenerTodos(int limite) =>
        Task.FromResult(_filas.Values.Where(f => f.Activo).Select(f => f.Usuario).Take(limite));

    public Task<Usuario?> ObtenerPorId(int id)
    {
        if (!_filas.TryGetValue(id, out var f) || !f.Activo) return Task.FromResult<Usuario?>(null);
        f.Usuario.Roles = f.Roles.ToList();
        return Task.FromResult<Usuario?>(f.Usuario);
    }

    public Task<int> Crear(Usuario usuario, string hashClave)
    {
        var id = _filas.Count == 0 ? 1 : _filas.Keys.Max() + 1;
        usuario.Id = id;
        _filas[id] = new Fila { Usuario = usuario, Hash = hashClave, Activo = true };
        return Task.FromResult(id);
    }

    public Task<int> Reemplazar(Usuario usuario) => Task.FromResult(Activo(usuario.Id) ? 1 : 0);

    public Task<int> ActualizarParcial(int id, UsuarioCambios cambios, string? nuevoHash)
    {
        if (!Activo(id)) return Task.FromResult(0);
        if (nuevoHash != null) _filas[id].Hash = nuevoHash;
        return Task.FromResult(1);
    }

    public Task<int> EliminarLogico(int id)
    {
        if (!Activo(id)) return Task.FromResult(0);
        _filas[id].Activo = false;
        return Task.FromResult(1);
    }

    public Task<CredencialesUsuario?> ObtenerCredenciales(string username)
    {
        var f = _filas.Values.FirstOrDefault(x => x.Usuario.Username == username);
        return Task.FromResult(f == null ? null : new CredencialesUsuario
        {
            Id = f.Usuario.Id, Username = f.Usuario.Username, Password = f.Hash, Activo = f.Activo
        });
    }

    public Task<List<string>> RolesActivos(int usuarioId) => Task.FromResult(_filas[usuarioId].Roles.ToList());

    public Task<int> ContarAdministradoresActivos() =>
        Task.FromResult(_filas.Values.Count(f => f.Activo && f.Roles.Contains(Roles.Administrador)));

    public Task RestablecerAdministrador(int id, string hashClave, string email)
    {
        _filas[id].Hash = hashClave;
        _filas[id].Activo = true;
        return Task.CompletedTask;
    }
}

public class RepositorioRolFalso : IRepositorioRol
{
    private readonly List<Rol> _filas = new()
    {
        new Rol { Id = 1, Nombre = Roles.Administrador },
        new Rol { Id = 2, Nombre = Roles.Editor },
        new Rol { Id = 3, Nombre = Roles.Consulta }
    };

    /// <summary>Los usuarios que tiene cada rol, para UsuariosActivosConRol.</summary>
    public Func<string, int> ContarUsuarios { get; set; } = _ => 0;

    public int IdDe(string nombre) => _filas.First(r => r.Nombre == nombre).Id;

    public Task<IEnumerable<Rol>> ObtenerTodos(int limite) => Task.FromResult(_filas.Take(limite).AsEnumerable());
    public Task<Rol?> ObtenerPorId(int id) => Task.FromResult(_filas.FirstOrDefault(r => r.Id == id));
    public Task<Rol?> ObtenerPorNombre(string nombre) => Task.FromResult(_filas.FirstOrDefault(r => r.Nombre == nombre));

    public Task<int> Crear(Rol rol)
    {
        rol.Id = _filas.Max(r => r.Id) + 1;
        _filas.Add(rol);
        return Task.FromResult(rol.Id);
    }

    public Task<int> Reemplazar(Rol rol) => Task.FromResult(_filas.Any(r => r.Id == rol.Id) ? 1 : 0);
    public Task<int> ActualizarParcial(int id, RolCambios cambios) => Task.FromResult(_filas.Any(r => r.Id == id) ? 1 : 0);
    public Task<int> EliminarLogico(int id) => Task.FromResult(_filas.RemoveAll(r => r.Id == id));

    public Task<int> UsuariosActivosConRol(int id) =>
        Task.FromResult(ContarUsuarios(_filas.First(r => r.Id == id).Nombre));
}

public class RepositorioRolUsuarioFalso : IRepositorioRolUsuario
{
    private readonly RepositorioUsuarioFalso _usuarios;
    private readonly RepositorioRolFalso _roles;

    public RepositorioRolUsuarioFalso(RepositorioUsuarioFalso usuarios, RepositorioRolFalso roles)
    {
        _usuarios = usuarios;
        _roles = roles;
        // Así el repositorio de roles «ve» las asignaciones de los usuarios.
        _roles.ContarUsuarios = nombre => Enumerable.Range(1, 100)
            .Count(id => _usuarios.Activo(id) && _usuarios.RolesDe(id).Contains(nombre));
    }

    private string Nombre(int rolId) => _roles.ObtenerPorId(rolId).Result!.Nombre;

    public Task<IEnumerable<RolUsuario>> ObtenerTodos(int limite) => Task.FromResult(Enumerable.Empty<RolUsuario>());

    public Task<RolUsuario?> ObtenerPorLlave(int usuarioId, int rolId) =>
        Task.FromResult(_usuarios.Activo(usuarioId) && _usuarios.RolesDe(usuarioId).Contains(Nombre(rolId))
            ? new RolUsuario { UsuarioId = usuarioId, RolId = rolId, Rol = Nombre(rolId) }
            : null);

    public Task<bool> Asignar(int usuarioId, int rolId)
    {
        if (_usuarios.RolesDe(usuarioId).Contains(Nombre(rolId))) return Task.FromResult(false);
        _usuarios.AsignarRol(usuarioId, Nombre(rolId));
        return Task.FromResult(true);
    }

    public Task<int> EliminarLogico(int usuarioId, int rolId)
    {
        var tenia = _usuarios.RolesDe(usuarioId).Contains(Nombre(rolId));
        _usuarios.QuitarRol(usuarioId, Nombre(rolId));
        return Task.FromResult(tenia ? 1 : 0);
    }
}
