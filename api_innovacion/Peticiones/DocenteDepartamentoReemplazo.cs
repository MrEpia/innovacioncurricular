using System.ComponentModel.DataAnnotations;

namespace ApiInnovacion.Peticiones;

/// <summary>
/// El cuerpo del PUT para docente_departamento: reemplazar es poner TODO de nuevo, así que los campos obligatorios lo son también aquí. La llave no va: identifica la fila y viaja en la ruta. Un campo opcional que no llegue queda en NULL.
///
/// Los tipos son anulables a propósito: si no lo fueran, un número ausente
/// llegaría como 0 y el [Required] no se enteraría.
/// </summary>
public class DocenteDepartamentoReemplazo
{
    [Required(ErrorMessage = "El campo dedicacion es obligatorio.")]
    [MaxLength(15, ErrorMessage = "El campo dedicacion no puede exceder los 15 caracteres.")]
    public string? Dedicacion { get; set; }

    [Required(ErrorMessage = "El campo modalidad es obligatorio.")]
    [MaxLength(45, ErrorMessage = "El campo modalidad no puede exceder los 45 caracteres.")]
    public string? Modalidad { get; set; }

    [Required(ErrorMessage = "El campo fechaIngreso es obligatorio.")]
    public DateTime? FechaIngreso { get; set; }

    public DateTime? FechaSalida { get; set; }
}
