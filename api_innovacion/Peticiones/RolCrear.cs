using System.ComponentModel.DataAnnotations;

namespace ApiInnovacion.Peticiones;

/// <summary>POST /api/rol. El id lo genera la base.</summary>
public class RolCrear
{
    [Required(ErrorMessage = "El campo nombre es obligatorio.")]
    [MaxLength(100, ErrorMessage = "El campo nombre no puede exceder los 100 caracteres.")]
    public string? Nombre { get; set; }

    public string? Descripcion { get; set; }
}
