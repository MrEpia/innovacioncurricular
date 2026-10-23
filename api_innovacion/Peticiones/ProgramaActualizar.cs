using System.ComponentModel.DataAnnotations;

namespace ApiInnovacion.Peticiones;

/// <summary>
/// El cuerpo del PATCH para programa: todo opcional, y solo se escribe lo que llegue. El MISMO cuerpo incompleto responde 422 en PUT y 200 aquí: lo decide el tipo, no un if.
///
/// Los tipos son anulables a propósito: si no lo fueran, un número ausente
/// llegaría como 0 y el [Required] no se enteraría.
/// </summary>
public class ProgramaActualizar
{
    [MaxLength(150, ErrorMessage = "El campo nombre no puede exceder los 150 caracteres.")]
    public string? Nombre { get; set; }

    [MaxLength(45, ErrorMessage = "El campo tipo no puede exceder los 45 caracteres.")]
    public string? Tipo { get; set; }

    [MaxLength(45, ErrorMessage = "El campo nivel no puede exceder los 45 caracteres.")]
    public string? Nivel { get; set; }

    [MaxLength(45, ErrorMessage = "El campo fechaCreacion no puede exceder los 45 caracteres.")]
    public string? FechaCreacion { get; set; }

    [MaxLength(45, ErrorMessage = "El campo fechaCierre no puede exceder los 45 caracteres.")]
    public string? FechaCierre { get; set; }

    [MaxLength(45, ErrorMessage = "El campo numeroCohortes no puede exceder los 45 caracteres.")]
    public string? NumeroCohortes { get; set; }

    [MaxLength(45, ErrorMessage = "El campo cantGraduados no puede exceder los 45 caracteres.")]
    public string? CantGraduados { get; set; }

    [MaxLength(45, ErrorMessage = "El campo fechaActualizacion no puede exceder los 45 caracteres.")]
    public string? FechaActualizacion { get; set; }

    [MaxLength(45, ErrorMessage = "El campo ciudad no puede exceder los 45 caracteres.")]
    public string? Ciudad { get; set; }

    public int? Facultad { get; set; }
}
