using System.ComponentModel.DataAnnotations;

namespace ApiInnovacion.Peticiones;

/// <summary>
/// El cuerpo del POST para enfoque_rc. Lleva la llave, porque en esta tabla la llave no la genera la base: la escribe quien registra. Si falta un campo obligatorio, el framework responde 422 antes de que el negocio se entere.
///
/// Los tipos son anulables a propósito: si no lo fueran, un número ausente
/// llegaría como 0 y el [Required] no se enteraría.
/// </summary>
public class EnfoqueRcCrear
{
    [Required(ErrorMessage = "El campo enfoque es obligatorio.")]
    public int? Enfoque { get; set; }

    [Required(ErrorMessage = "El campo registroCalificado es obligatorio.")]
    public int? RegistroCalificado { get; set; }
}
