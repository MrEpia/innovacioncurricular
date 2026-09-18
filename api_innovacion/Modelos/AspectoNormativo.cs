namespace ApiInnovacion.Modelos;

/// <summary>
/// Entidad de dominio que representa la tabla aspecto_normativo.
/// Es lo que viaja entre las capas de repositorio, servicio y controlador.
///
/// No incluye la propiedad Activo: el borrado lógico es un detalle interno del
/// motor y no forma parte de lo que la API expone.
/// </summary>
public class AspectoNormativo
{
    /// <summary>Llave primaria.</summary>
    public int Id { get; set; }

    public string Tipo { get; set; } = string.Empty;

    public string Descripcion { get; set; } = string.Empty;

    public string Fuente { get; set; } = string.Empty;
}
