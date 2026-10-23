using System.ComponentModel.DataAnnotations;

namespace ApiInnovacion.Peticiones;

/// <summary>
/// El cuerpo del PUT para acreditacion: reemplazar es poner TODO de nuevo, así que los campos obligatorios lo son también aquí. La llave no va: identifica la fila y viaja en la ruta. Un campo opcional que no llegue queda en NULL.
///
/// Los tipos son anulables a propósito: si no lo fueran, un número ausente
/// llegaría como 0 y el [Required] no se enteraría.
/// </summary>
public class AcreditacionReemplazo
{
    [Required(ErrorMessage = "El campo tipo es obligatorio.")]
    [MaxLength(45, ErrorMessage = "El campo tipo no puede exceder los 45 caracteres.")]
    public string? Tipo { get; set; }

    [Required(ErrorMessage = "El campo calificacion es obligatorio.")]
    [MaxLength(45, ErrorMessage = "El campo calificacion no puede exceder los 45 caracteres.")]
    public string? Calificacion { get; set; }

    [Required(ErrorMessage = "El campo fechaInicio es obligatorio.")]
    [MaxLength(45, ErrorMessage = "El campo fechaInicio no puede exceder los 45 caracteres.")]
    public string? FechaInicio { get; set; }

    [Required(ErrorMessage = "El campo fechaFin es obligatorio.")]
    [MaxLength(45, ErrorMessage = "El campo fechaFin no puede exceder los 45 caracteres.")]
    public string? FechaFin { get; set; }

    [Required(ErrorMessage = "El campo programa es obligatorio.")]
    public int? Programa { get; set; }
}
