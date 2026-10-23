using System.ComponentModel.DataAnnotations;

namespace ApiInnovacion.Peticiones;

/// <summary>
/// El cuerpo del PATCH para docente_departamento: todo opcional, y solo se escribe lo que llegue. El MISMO cuerpo incompleto responde 422 en PUT y 200 aquí: lo decide el tipo, no un if.
///
/// Los tipos son anulables a propósito: si no lo fueran, un número ausente
/// llegaría como 0 y el [Required] no se enteraría.
/// </summary>
public class DocenteDepartamentoActualizar
{
    [MaxLength(15, ErrorMessage = "El campo dedicacion no puede exceder los 15 caracteres.")]
    public string? Dedicacion { get; set; }

    [MaxLength(45, ErrorMessage = "El campo modalidad no puede exceder los 45 caracteres.")]
    public string? Modalidad { get; set; }

    public DateTime? FechaIngreso { get; set; }

    public DateTime? FechaSalida { get; set; }
}
