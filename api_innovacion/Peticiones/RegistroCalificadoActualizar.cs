using System.ComponentModel.DataAnnotations;

namespace ApiInnovacion.Peticiones;

/// <summary>
/// El cuerpo del PATCH para registro_calificado: todo opcional, y solo se escribe lo que llegue. El MISMO cuerpo incompleto responde 422 en PUT y 200 aquí: lo decide el tipo, no un if.
///
/// Los tipos son anulables a propósito: si no lo fueran, un número ausente
/// llegaría como 0 y el [Required] no se enteraría.
/// </summary>
public class RegistroCalificadoActualizar
{
    [MaxLength(45, ErrorMessage = "El campo cantCreditos no puede exceder los 45 caracteres.")]
    public string? CantCreditos { get; set; }

    [MaxLength(45, ErrorMessage = "El campo horaAcom no puede exceder los 45 caracteres.")]
    public string? HoraAcom { get; set; }

    [MaxLength(45, ErrorMessage = "El campo horaInd no puede exceder los 45 caracteres.")]
    public string? HoraInd { get; set; }

    [MaxLength(45, ErrorMessage = "El campo metodologia no puede exceder los 45 caracteres.")]
    public string? Metodologia { get; set; }

    public DateTime? FechaInicio { get; set; }

    public DateTime? FechaFin { get; set; }

    [MaxLength(45, ErrorMessage = "El campo duracionAnios no puede exceder los 45 caracteres.")]
    public string? DuracionAnios { get; set; }

    [MaxLength(45, ErrorMessage = "El campo duracionSemestres no puede exceder los 45 caracteres.")]
    public string? DuracionSemestres { get; set; }

    [MaxLength(45, ErrorMessage = "El campo tipoTitulacion no puede exceder los 45 caracteres.")]
    public string? TipoTitulacion { get; set; }

    public int? Programa { get; set; }
}
