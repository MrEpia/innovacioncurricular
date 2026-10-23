using System.ComponentModel.DataAnnotations;

namespace ApiInnovacion.Peticiones;

/// <summary>
/// El cuerpo del PATCH para activ_academica: todo opcional, y solo se escribe lo que llegue. El MISMO cuerpo incompleto responde 422 en PUT y 200 aquí: lo decide el tipo, no un if.
///
/// Los tipos son anulables a propósito: si no lo fueran, un número ausente
/// llegaría como 0 y el [Required] no se enteraría.
/// </summary>
public class ActivAcademicaActualizar
{
    [MaxLength(45, ErrorMessage = "El campo nombre no puede exceder los 45 caracteres.")]
    public string? Nombre { get; set; }

    public int? NumCreditos { get; set; }

    [MaxLength(20, ErrorMessage = "El campo tipo no puede exceder los 20 caracteres.")]
    public string? Tipo { get; set; }

    [MaxLength(45, ErrorMessage = "El campo areaFormacion no puede exceder los 45 caracteres.")]
    public string? AreaFormacion { get; set; }

    public int? HAcom { get; set; }

    public int? HIndep { get; set; }

    [MaxLength(45, ErrorMessage = "El campo idioma no puede exceder los 45 caracteres.")]
    public string? Idioma { get; set; }

    public byte? Espejo { get; set; }

    [MaxLength(45, ErrorMessage = "El campo entidadEspejo no puede exceder los 45 caracteres.")]
    public string? EntidadEspejo { get; set; }

    [MaxLength(45, ErrorMessage = "El campo paisEspejo no puede exceder los 45 caracteres.")]
    public string? PaisEspejo { get; set; }

    public int? Disenio { get; set; }
}
