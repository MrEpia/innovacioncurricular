namespace ApiInnovacion.Peticiones;

/// <summary>
/// El cuerpo del PATCH para practica_estrategia: los campos son OPCIONALES, y solo se
/// escriben los que lleguen.
///
/// La diferencia con PracticaEstrategiaReemplazo es toda la lección del contrato: el MISMO
/// cuerpo responde 422 en PUT y 200 en PATCH, y no lo decide un if en el
/// servicio — lo decide el tipo.
/// </summary>
public class PracticaEstrategiaActualizar
{
    public string? Tipo { get; set; }
    public string? Nombre { get; set; }
    public string? Descripcion { get; set; }
}
