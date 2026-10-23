using System.ComponentModel.DataAnnotations;

namespace ApiInnovacion.Peticiones;

/// <summary>
/// El cuerpo del PATCH para facultad: todo opcional, y solo se escribe lo que llegue. El MISMO cuerpo incompleto responde 422 en PUT y 200 aquí: lo decide el tipo, no un if.
///
/// Los tipos son anulables a propósito: si no lo fueran, un número ausente
/// llegaría como 0 y el [Required] no se enteraría.
/// </summary>
public class FacultadActualizar
{
    [MaxLength(60, ErrorMessage = "El campo nombre no puede exceder los 60 caracteres.")]
    public string? Nombre { get; set; }

    [MaxLength(45, ErrorMessage = "El campo tipo no puede exceder los 45 caracteres.")]
    public string? Tipo { get; set; }

    public DateTime? FechaFun { get; set; }

    public int? Universidad { get; set; }
}
