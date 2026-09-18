using System.ComponentModel.DataAnnotations;

namespace ApiInnovacion.Peticiones;

/// <summary>
/// El cuerpo del PUT para area_conocimiento. Reemplazar es poner TODO de nuevo, así que
/// todos los campos son obligatorios. El id no va aquí: identifica la
/// fila y viaja en la ruta.
/// </summary>
public class AreaConocimientoReemplazo
{
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
