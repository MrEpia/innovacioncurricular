namespace ApiInnovacion.Modelos;

/// <summary>
/// Entidad de la tabla usuario (v3), tal como la API la expone.
///
/// NO tiene la contraseña, a propósito: ni siquiera su hash sale nunca de la
/// API. El hash solo existe en CredencialesUsuario, que usa el ingreso y no
/// llega a ningún controlador.
/// </summary>
public class Usuario
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? NombreCompleto { get; set; }
    public DateTime? FechaCreacion { get; set; }
    public DateTime? FechaActualizacion { get; set; }

    /// <summary>Los roles ACTIVOS asignados. Se llenan al listar y al consultar.</summary>
    public List<string> Roles { get; set; } = new();
}

/// <summary>Los cambios de un PATCH sobre usuario. null = «no lo toque».</summary>
public class UsuarioCambios
{
    public string? Username { get; set; }
    public string? Email { get; set; }
    public string? NombreCompleto { get; set; }

    /// <summary>Contraseña NUEVA en claro: el servicio la hashea antes de guardarla.</summary>
    public string? Clave { get; set; }

    public bool HayAlgo() => Username != null || Email != null || NombreCompleto != null || Clave != null;
}

/// <summary>
/// Lo que necesita el ingreso, y solo el ingreso: el hash guardado y si el
/// usuario está activo. Nunca se serializa.
/// </summary>
public class CredencialesUsuario
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? NombreCompleto { get; set; }
    public string Password { get; set; } = string.Empty;
    public bool Activo { get; set; }
}
