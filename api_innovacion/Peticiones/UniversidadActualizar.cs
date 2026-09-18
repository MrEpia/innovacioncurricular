namespace ApiInnovacion.Peticiones;

/// <summary>
/// El cuerpo del PATCH para universidad: los campos son OPCIONALES, y solo se
/// escriben los que lleguen.
///
/// La diferencia con UniversidadReemplazo es toda la lección del contrato: el MISMO
/// cuerpo responde 422 en PUT y 200 en PATCH, y no lo decide un if en el
/// servicio — lo decide el tipo.
/// </summary>
public class UniversidadActualizar
{
    public string? Nombre { get; set; }
    public string? Tipo { get; set; }
    public string? Ciudad { get; set; }
}
