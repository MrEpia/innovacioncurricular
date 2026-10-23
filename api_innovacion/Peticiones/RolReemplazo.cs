using System.ComponentModel.DataAnnotations;

namespace ApiInnovacion.Peticiones;

/// <summary>PUT /api/rol/{id}.</summary>
public class RolReemplazo
{
    [Required(ErrorMessage = "El campo nombre es obligatorio.")]
    [MaxLength(100, ErrorMessage = "El campo nombre no puede exceder los 100 caracteres.")]
    public string? Nombre { get; set; }

    public string? Descripcion { get; set; }
}
