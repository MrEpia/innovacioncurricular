using System.ComponentModel.DataAnnotations;

namespace ApiInnovacion.Peticiones;

/// <summary>
/// El cuerpo del POST para acreditacion. Lleva la llave, porque en esta tabla la llave no la genera la base: la escribe quien registra. Si falta un campo obligatorio, el framework responde 422 antes de que el negocio se entere.
///
/// Los tipos son anulables a propósito: si no lo fueran, un número ausente
/// llegaría como 0 y el [Required] no se enteraría.
/// </summary>
public class AcreditacionCrear
{
    [Required(ErrorMessage = "El campo resolucion es obligatorio.")]
    public int? Resolucion { get; set; }

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
