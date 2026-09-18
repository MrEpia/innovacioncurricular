using System.ComponentModel.DataAnnotations;

namespace ApiInnovacion.Peticiones;

/// <summary>
/// El cuerpo del POST para area_conocimiento. Todos los campos son obligatorios: crear un
/// registro a medias no tiene sentido. Si falta uno, el framework responde 422
/// antes de que el negocio se entere.
/// </summary>
public class AreaConocimientoCrear
{
    [Required(ErrorMessage = "El campo id es obligatorio.")]
    [MaxLength(6, ErrorMessage = "El campo id no puede exceder los 6 caracteres.")]
    public string Id { get; set; } = string.Empty;

    [Required(ErrorMessage = "El campo granArea es obligatorio.")]
    [MaxLength(60, ErrorMessage = "El campo granArea no puede exceder los 60 caracteres.")]
    public string GranArea { get; set; } = string.Empty;

    [Required(ErrorMessage = "El campo area es obligatorio.")]
    [MaxLength(60, ErrorMessage = "El campo area no puede exceder los 60 caracteres.")]
    public string Area { get; set; } = string.Empty;

    [Required(ErrorMessage = "El campo disciplina es obligatorio.")]
    [MaxLength(150, ErrorMessage = "El campo disciplina no puede exceder los 150 caracteres.")]
    public string Disciplina { get; set; } = string.Empty;
}
