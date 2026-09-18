namespace ApiInnovacion.Modelos;

/// <summary>
/// Entidad de dominio que representa la tabla area_conocimiento.
/// Es lo que viaja entre las capas de repositorio, servicio y controlador.
///
/// No incluye la propiedad Activo: el borrado lógico es un detalle interno del
/// motor y no forma parte de lo que la API expone.
/// </summary>
public class AreaConocimiento
{
    /// <summary>Llave primaria.</summary>
    public string Id { get; set; } = string.Empty;

    public string GranArea { get; set; } = string.Empty;

    public string Area { get; set; } = string.Empty;

    public string Disciplina { get; set; } = string.Empty;
}
