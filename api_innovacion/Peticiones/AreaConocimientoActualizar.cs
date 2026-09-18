namespace ApiInnovacion.Peticiones;

/// <summary>
/// El cuerpo del PATCH para area_conocimiento: los campos son OPCIONALES, y solo se
/// escriben los que lleguen.
///
/// La diferencia con AreaConocimientoReemplazo es toda la lección del contrato: el MISMO
/// cuerpo responde 422 en PUT y 200 en PATCH, y no lo decide un if en el
/// servicio — lo decide el tipo.
/// </summary>
public class AreaConocimientoActualizar
{
    public string? GranArea { get; set; }
    public string? Area { get; set; }
    public string? Disciplina { get; set; }
}
