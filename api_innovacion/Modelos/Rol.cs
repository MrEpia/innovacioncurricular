namespace ApiInnovacion.Modelos;

/// <summary>Entidad de la tabla rol (v3). El id lo genera la base (IDENTITY).</summary>
public class Rol
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public DateTime? FechaCreacion { get; set; }
}

/// <summary>Los cambios de un PATCH sobre rol. null = «no lo toque».</summary>
public class RolCambios
{
    public string? Nombre { get; set; }
    public string? Descripcion { get; set; }

    public bool HayAlgo() => Nombre != null || Descripcion != null;
}

/// <summary>
/// Una asignación de la tabla rol_usuario. Además de las dos llaves trae el
/// nombre de usuario y el del rol (un JOIN), para que quien lee la lista no
/// tenga que cruzar números a mano.
/// </summary>
public class RolUsuario
{
    public int UsuarioId { get; set; }
    public int RolId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
}
