using ApiInnovacion.Modelos;

namespace ApiInnovacion.Repositorios;

/// <summary>
/// La capa 3 del maestro-detalle de registro_calificado. Una sola operación de
/// escritura: guardar el maestro y TODO su detalle juntos, o nada.
/// </summary>
public interface IRepositorioRegistroCalificadoCompleto
{
    /// <summary>
    /// Guarda el registro, sus enfoque_rc y sus aa_rc en una transacción.
    /// Lanza LlaveDuplicadaExcepcion (llave repetida) o
    /// ReferenciaInvalidaExcepcion (un padre se retiró entretanto).
    /// </summary>
    Task Crear(RegistroCalificadoCompleto entidad);

    /// <summary>El registro activo con su detalle activo, o null.</summary>
    Task<RegistroCalificadoCompleto?> ObtenerCompleto(int codigo);
}
