using System.ComponentModel.DataAnnotations;

namespace ApiInnovacion.Peticiones;

/// <summary>
/// El cuerpo del PATCH para acreditacion: todo opcional, y solo se escribe lo que llegue. El MISMO cuerpo incompleto responde 422 en PUT y 200 aquí: lo decide el tipo, no un if.
///
/// Los tipos son anulables a propósito: si no lo fueran, un número ausente
/// llegaría como 0 y el [Required] no se enteraría.
/// </summary>
public class AcreditacionActualizar
{
    [MaxLength(45, ErrorMessage = "El campo tipo no puede exceder los 45 caracteres.")]
    public string? Tipo { get; set; }

    [MaxLength(45, ErrorMessage = "El campo calificacion no puede exceder los 45 caracteres.")]
    public string? Calificacion { get; set; }

    [MaxLength(45, ErrorMessage = "El campo fechaInicio no puede exceder los 45 caracteres.")]
    public string? FechaInicio { get; set; }

    [MaxLength(45, ErrorMessage = "El campo fechaFin no puede exceder los 45 caracteres.")]
    public string? FechaFin { get; set; }

    public int? Programa { get; set; }
}
