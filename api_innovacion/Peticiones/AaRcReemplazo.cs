using System.ComponentModel.DataAnnotations;

namespace ApiInnovacion.Peticiones;

/// <summary>
/// El cuerpo del PUT para aa_rc: reemplazar es poner TODO de nuevo, así que los campos obligatorios lo son también aquí. La llave no va: identifica la fila y viaja en la ruta. Un campo opcional que no llegue queda en NULL.
///
/// Los tipos son anulables a propósito: si no lo fueran, un número ausente
/// llegaría como 0 y el [Required] no se enteraría.
/// </summary>
public class AaRcReemplazo
{
    [Required(ErrorMessage = "El campo componente es obligatorio.")]
    [MaxLength(45, ErrorMessage = "El campo componente no puede exceder los 45 caracteres.")]
    public string? Componente { get; set; }

    [Required(ErrorMessage = "El campo semestre es obligatorio.")]
    [MaxLength(45, ErrorMessage = "El campo semestre no puede exceder los 45 caracteres.")]
    public string? Semestre { get; set; }
}
