namespace ApiInnovacion.Modelos;

/// <summary>
/// Entidad de dominio que representa la tabla car_innovacion.
/// Es lo que viaja entre las capas de repositorio, servicio y controlador.
///
/// No incluye la propiedad Activo: el borrado lógico es un detalle interno del
/// motor y no forma parte de lo que la API expone.
/// </summary>
public class CarInnovacion
{
    /// <summary>Llave primaria.</summary>
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string Descripcion { get; set; } = string.Empty;

    public string Tipo { get; set; } = string.Empty;
}
