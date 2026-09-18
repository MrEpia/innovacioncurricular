namespace ApiInnovacion.Peticiones;

/// <summary>
/// El cuerpo del PATCH para aspecto_normativo: los campos son OPCIONALES, y solo se
/// escriben los que lleguen.
///
/// La diferencia con AspectoNormativoReemplazo es toda la lección del contrato: el MISMO
/// cuerpo responde 422 en PUT y 200 en PATCH, y no lo decide un if en el
/// servicio — lo decide el tipo.
/// </summary>
public class AspectoNormativoActualizar
{
    public string? Tipo { get; set; }
    public string? Descripcion { get; set; }
    public string? Fuente { get; set; }
}
