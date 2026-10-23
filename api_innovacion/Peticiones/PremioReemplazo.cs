using System.ComponentModel.DataAnnotations;

namespace ApiInnovacion.Peticiones;

/// <summary>
/// El cuerpo del PUT para premio: reemplazar es poner TODO de nuevo, así que los campos obligatorios lo son también aquí. La llave no va: identifica la fila y viaja en la ruta. Un campo opcional que no llegue queda en NULL.
///
/// Los tipos son anulables a propósito: si no lo fueran, un número ausente
/// llegaría como 0 y el [Required] no se enteraría.
/// </summary>
public class PremioReemplazo
{
    [Required(ErrorMessage = "El campo nombre es obligatorio.")]
    [MaxLength(45, ErrorMessage = "El campo nombre no puede exceder los 45 caracteres.")]
    public string? Nombre { get; set; }

    [Required(ErrorMessage = "El campo descripcion es obligatorio.")]
    [MaxLength(45, ErrorMessage = "El campo descripcion no puede exceder los 45 caracteres.")]
    public string? Descripcion { get; set; }

    [Required(ErrorMessage = "El campo fecha es obligatorio.")]
    public DateTime? Fecha { get; set; }

    [Required(ErrorMessage = "El campo entidadOtorgante es obligatorio.")]
    [MaxLength(45, ErrorMessage = "El campo entidadOtorgante no puede exceder los 45 caracteres.")]
    public string? EntidadOtorgante { get; set; }

    [Required(ErrorMessage = "El campo pais es obligatorio.")]
    [MaxLength(45, ErrorMessage = "El campo pais no puede exceder los 45 caracteres.")]
    public string? Pais { get; set; }

    [Required(ErrorMessage = "El campo programa es obligatorio.")]
    public int? Programa { get; set; }
}
