using System.ComponentModel.DataAnnotations;

namespace ApiInnovacion.Peticiones;

/// <summary>
/// El cuerpo del POST para programa_ac. Lleva la llave, porque en esta tabla la llave no la genera la base: la escribe quien registra. Si falta un campo obligatorio, el framework responde 422 antes de que el negocio se entere.
///
/// Los tipos son anulables a propósito: si no lo fueran, un número ausente
/// llegaría como 0 y el [Required] no se enteraría.
/// </summary>
public class ProgramaAcCrear
{
    [Required(ErrorMessage = "El campo programa es obligatorio.")]
    public int? Programa { get; set; }

    [Required(ErrorMessage = "El campo areaConocimiento es obligatorio.")]
    [MaxLength(6, ErrorMessage = "El campo areaConocimiento no puede exceder los 6 caracteres.")]
    public string? AreaConocimiento { get; set; }
}
