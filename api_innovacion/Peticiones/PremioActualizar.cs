using System.ComponentModel.DataAnnotations;

namespace ApiInnovacion.Peticiones;

/// <summary>
/// El cuerpo del PATCH para premio: todo opcional, y solo se escribe lo que llegue. El MISMO cuerpo incompleto responde 422 en PUT y 200 aquí: lo decide el tipo, no un if.
///
/// Los tipos son anulables a propósito: si no lo fueran, un número ausente
/// llegaría como 0 y el [Required] no se enteraría.
/// </summary>
public class PremioActualizar
{
    [MaxLength(45, ErrorMessage = "El campo nombre no puede exceder los 45 caracteres.")]
    public string? Nombre { get; set; }

    [MaxLength(45, ErrorMessage = "El campo descripcion no puede exceder los 45 caracteres.")]
    public string? Descripcion { get; set; }

    public DateTime? Fecha { get; set; }

    [MaxLength(45, ErrorMessage = "El campo entidadOtorgante no puede exceder los 45 caracteres.")]
    public string? EntidadOtorgante { get; set; }

    [MaxLength(45, ErrorMessage = "El campo pais no puede exceder los 45 caracteres.")]
    public string? Pais { get; set; }

    public int? Programa { get; set; }
}
