using System.ComponentModel.DataAnnotations;

namespace ApiInnovacion.Peticiones;

/// <summary>El cuerpo de POST /api/login.</summary>
public class LoginPeticion
{
    [Required(ErrorMessage = "El campo usuario es obligatorio.")]
    public string? Usuario { get; set; }

    [Required(ErrorMessage = "El campo clave es obligatorio.")]
    public string? Clave { get; set; }
}
