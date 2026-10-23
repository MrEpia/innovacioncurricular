using System.ComponentModel.DataAnnotations;

namespace ApiInnovacion.Peticiones;

/// <summary>POST /api/usuario. La clave llega en claro por HTTPS y se guarda hasheada.</summary>
public class UsuarioCrear
{
    [Required(ErrorMessage = "El campo username es obligatorio.")]
    [MaxLength(100, ErrorMessage = "El campo username no puede exceder los 100 caracteres.")]
    public string? Username { get; set; }

    [Required(ErrorMessage = "El campo clave es obligatorio.")]
    [MinLength(8, ErrorMessage = "El campo clave debe tener al menos 8 caracteres.")]
    [MaxLength(100, ErrorMessage = "El campo clave no puede exceder los 100 caracteres.")]
    public string? Clave { get; set; }

    [Required(ErrorMessage = "El campo email es obligatorio.")]
    [EmailAddress(ErrorMessage = "El campo email no es un correo válido.")]
    [MaxLength(150, ErrorMessage = "El campo email no puede exceder los 150 caracteres.")]
    public string? Email { get; set; }

    [MaxLength(200, ErrorMessage = "El campo nombreCompleto no puede exceder los 200 caracteres.")]
    public string? NombreCompleto { get; set; }
}
