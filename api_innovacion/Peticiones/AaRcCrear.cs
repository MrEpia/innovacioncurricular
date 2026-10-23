using System.ComponentModel.DataAnnotations;

namespace ApiInnovacion.Peticiones;

/// <summary>
/// El cuerpo del POST para aa_rc. Lleva la llave, porque en esta tabla la llave no la genera la base: la escribe quien registra. Si falta un campo obligatorio, el framework responde 422 antes de que el negocio se entere.
///
/// Los tipos son anulables a propósito: si no lo fueran, un número ausente
/// llegaría como 0 y el [Required] no se enteraría.
/// </summary>
public class AaRcCrear
{
    [Required(ErrorMessage = "El campo activAcademicasIdcurso es obligatorio.")]
    public int? ActivAcademicasIdcurso { get; set; }

    [Required(ErrorMessage = "El campo registroCalificadoCodigo es obligatorio.")]
    public int? RegistroCalificadoCodigo { get; set; }

    [Required(ErrorMessage = "El campo componente es obligatorio.")]
    [MaxLength(45, ErrorMessage = "El campo componente no puede exceder los 45 caracteres.")]
    public string? Componente { get; set; }

    [Required(ErrorMessage = "El campo semestre es obligatorio.")]
    [MaxLength(45, ErrorMessage = "El campo semestre no puede exceder los 45 caracteres.")]
    public string? Semestre { get; set; }
}
