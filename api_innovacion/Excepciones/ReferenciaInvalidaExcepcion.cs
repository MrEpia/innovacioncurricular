namespace ApiInnovacion.Excepciones;

/// <summary>
/// Una clave foránea del cuerpo apunta a un registro que no existe o que está
/// inactivo («la facultad 99 no existe»).
///
/// Hereda de ArgumentException a propósito: para el contrato es una REGLA DE
/// NEGOCIO rota —el dato tiene la forma correcta, es un entero, pero no
/// señala nada válido—, y esas responden 400 (Artículo 10). Así el controlador
/// no necesita un catch nuevo para traducirla.
///
/// Por qué la API valida antes y no deja que lo rechace la FK del motor: porque
/// el motor SÍ acepta una fila hija que apunte a un padre INACTIVO —la fila
/// existe, solo está marcada—, y eso es justamente un huérfano lógico.
/// </summary>
public class ReferenciaInvalidaExcepcion : ArgumentException
{
    public ReferenciaInvalidaExcepcion(string mensaje) : base(mensaje)
    {
    }
}
