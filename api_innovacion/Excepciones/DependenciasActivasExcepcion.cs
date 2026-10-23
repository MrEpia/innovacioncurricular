namespace ApiInnovacion.Excepciones;

/// <summary>
/// Se intentó retirar (borrado lógico) un registro que todavía tiene registros
/// ACTIVOS que dependen de él: una facultad con programas, un programa con
/// acreditaciones.
///
/// El motor no lo impediría —el DELETE es un UPDATE y la FK no se entera—, y el
/// resultado serían hijos activos colgando de un padre que ya no aparece en
/// ninguna lista: registros huérfanos. El controlador la traduce a 409
/// Conflict (enmienda 1.1.0): la petición está bien formada, pero choca con el
/// estado actual de los datos.
/// </summary>
public class DependenciasActivasExcepcion : Exception
{
    public DependenciasActivasExcepcion(string mensaje) : base(mensaje)
    {
    }
}
