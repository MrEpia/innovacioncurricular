using System.ComponentModel.DataAnnotations;

namespace ApiInnovacion.Peticiones;

/// <summary>
/// El cuerpo del POST para docente_departamento. Lleva la llave, porque en esta tabla la llave no la genera la base: la escribe quien registra. Si falta un campo obligatorio, el framework responde 422 antes de que el negocio se entere.
///
/// Los tipos son anulables a propósito: si no lo fueran, un número ausente
/// llegaría como 0 y el [Required] no se enteraría.
/// </summary>
public class DocenteDepartamentoCrear
{
    [Required(ErrorMessage = "El campo docente es obligatorio.")]
    public int? Docente { get; set; }

    [Required(ErrorMessage = "El campo departamento es obligatorio.")]
    public int? Departamento { get; set; }

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
