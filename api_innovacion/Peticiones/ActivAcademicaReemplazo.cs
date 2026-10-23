using System.ComponentModel.DataAnnotations;

namespace ApiInnovacion.Peticiones;

/// <summary>
/// El cuerpo del PUT para activ_academica: reemplazar es poner TODO de nuevo, así que los campos obligatorios lo son también aquí. La llave no va: identifica la fila y viaja en la ruta. Un campo opcional que no llegue queda en NULL.
///
/// Los tipos son anulables a propósito: si no lo fueran, un número ausente
/// llegaría como 0 y el [Required] no se enteraría.
/// </summary>
public class ActivAcademicaReemplazo
{
    [Required(ErrorMessage = "El campo nombre es obligatorio.")]
    [MaxLength(45, ErrorMessage = "El campo nombre no puede exceder los 45 caracteres.")]
    public string? Nombre { get; set; }

    [Required(ErrorMessage = "El campo numCreditos es obligatorio.")]
    public int? NumCreditos { get; set; }

    [Required(ErrorMessage = "El campo tipo es obligatorio.")]
    [MaxLength(20, ErrorMessage = "El campo tipo no puede exceder los 20 caracteres.")]
    public string? Tipo { get; set; }

    [Required(ErrorMessage = "El campo areaFormacion es obligatorio.")]
    [MaxLength(45, ErrorMessage = "El campo areaFormacion no puede exceder los 45 caracteres.")]
    public string? AreaFormacion { get; set; }

    [Required(ErrorMessage = "El campo hAcom es obligatorio.")]
    public int? HAcom { get; set; }

    [Required(ErrorMessage = "El campo hIndep es obligatorio.")]
    public int? HIndep { get; set; }

    [Required(ErrorMessage = "El campo idioma es obligatorio.")]
    [MaxLength(45, ErrorMessage = "El campo idioma no puede exceder los 45 caracteres.")]
    public string? Idioma { get; set; }

    [Required(ErrorMessage = "El campo espejo es obligatorio.")]
    public byte? Espejo { get; set; }

    [Required(ErrorMessage = "El campo entidadEspejo es obligatorio.")]
    [MaxLength(45, ErrorMessage = "El campo entidadEspejo no puede exceder los 45 caracteres.")]
    public string? EntidadEspejo { get; set; }

    [Required(ErrorMessage = "El campo paisEspejo es obligatorio.")]
    [MaxLength(45, ErrorMessage = "El campo paisEspejo no puede exceder los 45 caracteres.")]
    public string? PaisEspejo { get; set; }

    public int? Disenio { get; set; }
}
