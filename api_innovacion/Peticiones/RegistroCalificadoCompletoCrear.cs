using System.ComponentModel.DataAnnotations;

namespace ApiInnovacion.Peticiones;

/// <summary>
/// El cuerpo de POST /api/registro_calificado/completo: los campos de
/// RegistroCalificadoCrear (heredados, con sus mismas validaciones) más el
/// detalle. [ApiController] valida también cada elemento de las listas:
/// una actividad sin componente es un 422, igual que en POST /api/aa_rc.
/// </summary>
public class RegistroCalificadoCompletoCrear : RegistroCalificadoCrear
{
    /// <summary>Ids de enfoque. Cada uno será una fila de enfoque_rc.</summary>
    public List<int> Enfoques { get; set; } = new();

    /// <summary>Cada una será una fila de aa_rc.</summary>
    public List<ActividadDelRegistroCrear> Actividades { get; set; } = new();
}

public class ActividadDelRegistroCrear
{
    [Required(ErrorMessage = "El campo actividades[].activAcademicasIdcurso es obligatorio.")]
    public int? ActivAcademicasIdcurso { get; set; }

    [Required(ErrorMessage = "El campo actividades[].componente es obligatorio.")]
    [MaxLength(45, ErrorMessage = "El campo actividades[].componente no puede exceder los 45 caracteres.")]
    public string? Componente { get; set; }

    [Required(ErrorMessage = "El campo actividades[].semestre es obligatorio.")]
    [MaxLength(45, ErrorMessage = "El campo actividades[].semestre no puede exceder los 45 caracteres.")]
    public string? Semestre { get; set; }
}
