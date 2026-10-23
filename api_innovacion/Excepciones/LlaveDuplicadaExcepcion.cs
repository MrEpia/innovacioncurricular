namespace ApiInnovacion.Excepciones;

/// <summary>
/// La llave primaria que se quiere crear ya existe (activa o inactiva).
///
/// La lanza el REPOSITORIO al recibir el error 2627/2601 de SQL Server: quien
/// se entera del choque es el motor, y traducir su número de error a un idioma
/// que las capas de arriba entiendan es trabajo de la capa de datos. El
/// controlador la convierte en 409 (enmienda 1.1.0).
///
/// Aplica a las tablas de la v2. La v1 decidió en su C11 dejar ese caso en
/// 500, y es una versión cerrada: su comportamiento no se toca aquí.
/// </summary>
public class LlaveDuplicadaExcepcion : Exception
{
    public LlaveDuplicadaExcepcion(string mensaje) : base(mensaje)
    {
    }
}
