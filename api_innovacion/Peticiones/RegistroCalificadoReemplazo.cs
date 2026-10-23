using System.ComponentModel.DataAnnotations;

namespace ApiInnovacion.Peticiones;

/// <summary>
/// El cuerpo del PUT para registro_calificado: reemplazar es poner TODO de nuevo, así que los campos obligatorios lo son también aquí. La llave no va: identifica la fila y viaja en la ruta. Un campo opcional que no llegue queda en NULL.
///
/// Los tipos son anulables a propósito: si no lo fueran, un número ausente
/// llegaría como 0 y el [Required] no se enteraría.
/// </summary>
public class RegistroCalificadoReemplazo
{
    [Required(ErrorMessage = "El campo cantCreditos es obligatorio.")]
    [MaxLength(45, ErrorMessage = "El campo cantCreditos no puede exceder los 45 caracteres.")]
    public string? CantCreditos { get; set; }

    [Required(ErrorMessage = "El campo horaAcom es obligatorio.")]
    [MaxLength(45, ErrorMessage = "El campo horaAcom no puede exceder los 45 caracteres.")]
    public string? HoraAcom { get; set; }

    [Required(ErrorMessage = "El campo horaInd es obligatorio.")]
    [MaxLength(45, ErrorMessage = "El campo horaInd no puede exceder los 45 caracteres.")]
    public string? HoraInd { get; set; }

    [Required(ErrorMessage = "El campo metodologia es obligatorio.")]
    [MaxLength(45, ErrorMessage = "El campo metodologia no puede exceder los 45 caracteres.")]
    public string? Metodologia { get; set; }

    [Required(ErrorMessage = "El campo fechaInicio es obligatorio.")]
    public DateTime? FechaInicio { get; set; }

    [Required(ErrorMessage = "El campo fechaFin es obligatorio.")]
    public DateTime? FechaFin { get; set; }

    [Required(ErrorMessage = "El campo duracionAnios es obligatorio.")]
    [MaxLength(45, ErrorMessage = "El campo duracionAnios no puede exceder los 45 caracteres.")]
    public string? DuracionAnios { get; set; }

    [Required(ErrorMessage = "El campo duracionSemestres es obligatorio.")]
    [MaxLength(45, ErrorMessage = "El campo duracionSemestres no puede exceder los 45 caracteres.")]
    public string? DuracionSemestres { get; set; }

    [Required(ErrorMessage = "El campo tipoTitulacion es obligatorio.")]
    [MaxLength(45, ErrorMessage = "El campo tipoTitulacion no puede exceder los 45 caracteres.")]
    public string? TipoTitulacion { get; set; }

    [Required(ErrorMessage = "El campo programa es obligatorio.")]
    public int? Programa { get; set; }
}
