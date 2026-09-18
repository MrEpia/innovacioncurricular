using System.ComponentModel.DataAnnotations;

namespace ApiInnovacion.Peticiones;

/// <summary>
/// El cuerpo del PUT para aspecto_normativo. Reemplazar es poner TODO de nuevo, así que
/// todos los campos son obligatorios. El id no va aquí: identifica la
/// fila y viaja en la ruta.
/// </summary>
public class AspectoNormativoReemplazo
{
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
