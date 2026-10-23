using System.ComponentModel.DataAnnotations;

namespace ApiInnovacion.Peticiones;

/// <summary>
/// El cuerpo del PATCH para alianza: todo opcional, y solo se escribe lo que llegue. El MISMO cuerpo incompleto responde 422 en PUT y 200 aquí: lo decide el tipo, no un if.
///
/// Los tipos son anulables a propósito: si no lo fueran, un número ausente
/// llegaría como 0 y el [Required] no se enteraría.
/// </summary>
public class AlianzaActualizar
{
    public DateTime? FechaInicio { get; set; }

    public DateTime? FechaFin { get; set; }

    public int? Docente { get; set; }
}
