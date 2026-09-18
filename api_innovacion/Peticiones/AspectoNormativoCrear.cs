using System.ComponentModel.DataAnnotations;

namespace ApiInnovacion.Peticiones;

/// <summary>
/// El cuerpo del POST para aspecto_normativo. Todos los campos son obligatorios: crear un
/// registro a medias no tiene sentido. Si falta uno, el framework responde 422
/// antes de que el negocio se entere.
/// </summary>
public class AspectoNormativoCrear
{
    [Required(ErrorMessage = "El campo id es obligatorio.")]
    public int? Id { get; set; }

    [Required(ErrorMessage = "El campo tipo es obligatorio.")]
    [MaxLength(45, ErrorMessage = "El campo tipo no puede exceder los 45 caracteres.")]
    public string Tipo { get; set; } = string.Empty;

    [Required(ErrorMessage = "El campo descripcion es obligatorio.")]
    [MaxLength(45, ErrorMessage = "El campo descripcion no puede exceder los 45 caracteres.")]
    public string Descripcion { get; set; } = string.Empty;

    [Required(ErrorMessage = "El campo fuente es obligatorio.")]
    [MaxLength(45, ErrorMessage = "El campo fuente no puede exceder los 45 caracteres.")]
    public string Fuente { get; set; } = string.Empty;
}
