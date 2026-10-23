using System.ComponentModel.DataAnnotations;

namespace ApiInnovacion.Peticiones;

/// <summary>
/// El cuerpo del PUT para programa: reemplazar es poner TODO de nuevo, así que los campos obligatorios lo son también aquí. La llave no va: identifica la fila y viaja en la ruta. Un campo opcional que no llegue queda en NULL.
///
/// Los tipos son anulables a propósito: si no lo fueran, un número ausente
/// llegaría como 0 y el [Required] no se enteraría.
/// </summary>
public class ProgramaReemplazo
{
    [Required(ErrorMessage = "El campo nombre es obligatorio.")]
    [MaxLength(150, ErrorMessage = "El campo nombre no puede exceder los 150 caracteres.")]
    public string? Nombre { get; set; }

    [Required(ErrorMessage = "El campo tipo es obligatorio.")]
    [MaxLength(45, ErrorMessage = "El campo tipo no puede exceder los 45 caracteres.")]
    public string? Tipo { get; set; }

    [Required(ErrorMessage = "El campo nivel es obligatorio.")]
    [MaxLength(45, ErrorMessage = "El campo nivel no puede exceder los 45 caracteres.")]
    public string? Nivel { get; set; }

    [Required(ErrorMessage = "El campo fechaCreacion es obligatorio.")]
    [MaxLength(45, ErrorMessage = "El campo fechaCreacion no puede exceder los 45 caracteres.")]
    public string? FechaCreacion { get; set; }

    [MaxLength(45, ErrorMessage = "El campo fechaCierre no puede exceder los 45 caracteres.")]
    public string? FechaCierre { get; set; }

    [Required(ErrorMessage = "El campo numeroCohortes es obligatorio.")]
    [MaxLength(45, ErrorMessage = "El campo numeroCohortes no puede exceder los 45 caracteres.")]
    public string? NumeroCohortes { get; set; }

    [Required(ErrorMessage = "El campo cantGraduados es obligatorio.")]
    [MaxLength(45, ErrorMessage = "El campo cantGraduados no puede exceder los 45 caracteres.")]
    public string? CantGraduados { get; set; }

    [Required(ErrorMessage = "El campo fechaActualizacion es obligatorio.")]
    [MaxLength(45, ErrorMessage = "El campo fechaActualizacion no puede exceder los 45 caracteres.")]
    public string? FechaActualizacion { get; set; }

    [Required(ErrorMessage = "El campo ciudad es obligatorio.")]
    [MaxLength(45, ErrorMessage = "El campo ciudad no puede exceder los 45 caracteres.")]
    public string? Ciudad { get; set; }

    [Required(ErrorMessage = "El campo facultad es obligatorio.")]
    public int? Facultad { get; set; }
}
