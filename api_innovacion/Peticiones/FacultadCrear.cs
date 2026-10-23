using System.ComponentModel.DataAnnotations;

namespace ApiInnovacion.Peticiones;

/// <summary>
/// El cuerpo del POST para facultad. Lleva la llave, porque en esta tabla la llave no la genera la base: la escribe quien registra. Si falta un campo obligatorio, el framework responde 422 antes de que el negocio se entere.
///
/// Los tipos son anulables a propósito: si no lo fueran, un número ausente
/// llegaría como 0 y el [Required] no se enteraría.
/// </summary>
public class FacultadCrear
{
    [Required(ErrorMessage = "El campo id es obligatorio.")]
    public int? Id { get; set; }

    [Required(ErrorMessage = "El campo nombre es obligatorio.")]
    [MaxLength(60, ErrorMessage = "El campo nombre no puede exceder los 60 caracteres.")]
    public string? Nombre { get; set; }

    [Required(ErrorMessage = "El campo tipo es obligatorio.")]
    [MaxLength(45, ErrorMessage = "El campo tipo no puede exceder los 45 caracteres.")]
    public string? Tipo { get; set; }

    [Required(ErrorMessage = "El campo fechaFun es obligatorio.")]
    public DateTime? FechaFun { get; set; }

    [Required(ErrorMessage = "El campo universidad es obligatorio.")]
    public int? Universidad { get; set; }
}
