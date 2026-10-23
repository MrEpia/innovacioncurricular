using System.ComponentModel.DataAnnotations;

namespace ApiInnovacion.Peticiones;

/// <summary>
/// El cuerpo del PUT para alianza: reemplazar es poner TODO de nuevo, así que los campos obligatorios lo son también aquí. La llave no va: identifica la fila y viaja en la ruta. Un campo opcional que no llegue queda en NULL.
///
/// Los tipos son anulables a propósito: si no lo fueran, un número ausente
/// llegaría como 0 y el [Required] no se enteraría.
/// </summary>
public class AlianzaReemplazo
{
    [Required(ErrorMessage = "El campo fechaInicio es obligatorio.")]
    public DateTime? FechaInicio { get; set; }

    public DateTime? FechaFin { get; set; }

    public int? Docente { get; set; }
}
