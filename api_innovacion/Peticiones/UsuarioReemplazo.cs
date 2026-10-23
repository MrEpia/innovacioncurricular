using System.ComponentModel.DataAnnotations;

namespace ApiInnovacion.Peticiones;

/// <summary>PUT /api/usuario/{id}: los datos de la ficha. La contraseña NO va aquí: se cambia por PATCH.</summary>
public class UsuarioReemplazo
{
    [Required(ErrorMessage = "El campo username es obligatorio.")]
    [MaxLength(100, ErrorMessage = "El campo username no puede exceder los 100 caracteres.")]
    public string? Username { get; set; }

    [Required(ErrorMessage = "El campo email es obligatorio.")]
    [EmailAddress(ErrorMessage = "El campo email no es un correo válido.")]
    [MaxLength(150, ErrorMessage = "El campo email no puede exceder los 150 caracteres.")]
    public string? Email { get; set; }

    [MaxLength(200, ErrorMessage = "El campo nombreCompleto no puede exceder los 200 caracteres.")]
    public string? NombreCompleto { get; set; }
}
