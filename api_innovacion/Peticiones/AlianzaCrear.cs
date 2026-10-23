using System.ComponentModel.DataAnnotations;

namespace ApiInnovacion.Peticiones;

/// <summary>
/// El cuerpo del POST para alianza. Lleva la llave, porque en esta tabla la llave no la genera la base: la escribe quien registra. Si falta un campo obligatorio, el framework responde 422 antes de que el negocio se entere.
///
/// Los tipos son anulables a propósito: si no lo fueran, un número ausente
/// llegaría como 0 y el [Required] no se enteraría.
/// </summary>
public class AlianzaCrear
{
    [Required(ErrorMessage = "El campo aliado es obligatorio.")]
    public int? Aliado { get; set; }

    [Required(ErrorMessage = "El campo departamento es obligatorio.")]
    public int? Departamento { get; set; }

    [Required(ErrorMessage = "El campo fechaInicio es obligatorio.")]
    public DateTime? FechaInicio { get; set; }

    public DateTime? FechaFin { get; set; }

    public int? Docente { get; set; }
}
