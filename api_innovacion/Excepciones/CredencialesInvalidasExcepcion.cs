namespace ApiInnovacion.Excepciones;

/// <summary>
/// El ingreso falló: usuario inexistente, contraseña equivocada o usuario
/// retirado. El controlador responde 401.
///
/// Los tres casos llevan EL MISMO mensaje a propósito: si la API dijera «ese
/// usuario no existe», un atacante podría averiguar qué usuarios existen
/// probando nombres (enumeración de usuarios). Quien sí debe saber la causa
/// —el administrador— la ve en el panel de usuarios.
/// </summary>
public class CredencialesInvalidasExcepcion : Exception
{
    public CredencialesInvalidasExcepcion()
        : base("Usuario o contraseña incorrectos.")
    {
    }
}

/// <summary>
/// La operación está bien formada pero el sistema no la permite por una regla
/// de seguridad: retirar al único administrador activo, retirar el rol
/// administrador, ingresar sin ningún rol. El controlador responde 409 (o 403
/// en el ingreso).
/// </summary>
public class OperacionNoPermitidaExcepcion : Exception
{
    public OperacionNoPermitidaExcepcion(string mensaje) : base(mensaje)
    {
    }
}
