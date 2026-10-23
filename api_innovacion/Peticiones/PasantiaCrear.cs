using System.ComponentModel.DataAnnotations;

namespace ApiInnovacion.Peticiones;

/// <summary>
/// El cuerpo del POST para pasantia. Lleva la llave, porque en esta tabla la llave no la genera la base: la escribe quien registra. Si falta un campo obligatorio, el framework responde 422 antes de que el negocio se entere.
///
/// Los tipos son anulables a propósito: si no lo fueran, un número ausente
/// llegaría como 0 y el [Required] no se enteraría.
/// </summary>
public class PasantiaCrear
{
    [Required(ErrorMessage = "El campo id es obligatorio.")]
    public int? Id { get; set; }

    [Required(ErrorMessage = "El campo nombre es obligatorio.")]
    [MaxLength(45, ErrorMessage = "El campo nombre no puede exceder los 45 caracteres.")]
    public string? Nombre { get; set; }

    [Required(ErrorMessage = "El campo pais es obligatorio.")]
    [MaxLength(45, ErrorMessage = "El campo pais no puede exceder los 45 caracteres.")]
    public string? Pais { get; set; }

    [Required(ErrorMessage = "El campo empresa es obligatorio.")]
    [MaxLength(45, ErrorMessage = "El campo empresa no puede exceder los 45 caracteres.")]
    public string? Empresa { get; set; }

    [Required(ErrorMessage = "El campo descripcion es obligatorio.")]
    [MaxLength(45, ErrorMessage = "El campo descripcion no puede exceder los 45 caracteres.")]
    public string? Descripcion { get; set; }

    [Required(ErrorMessage = "El campo programa es obligatorio.")]
    public int? Programa { get; set; }
}
