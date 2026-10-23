using ApiInnovacion.Modelos;

namespace ApiInnovacion.Servicios;

/// <summary>
/// El contrato de la capa de negocio para aa_rc. El controlador depende
/// de esta interfaz y no sabe qué hay detrás.
/// </summary>
public interface IServicioAaRc
{
    /// <summary>Hasta 'limite' registros activos. ArgumentException si limite &lt;= 0.</summary>
    Task<IEnumerable<AaRc>> ObtenerTodos(int limite);

    /// <summary>El registro con esa llave. NoEncontradoExcepcion si no existe o está inactivo.</summary>
    Task<AaRc> ObtenerPorLlave(int activAcademicasIdcurso, int registroCalificadoCodigo);

    /// <summary>Crea el registro. ReferenciaInvalidaExcepcion (400) si una clave foránea no apunta
    /// a un registro activo · LlaveDuplicadaExcepcion (409) si la llave ya existe.</summary>
    Task Crear(AaRc entidad);

    /// <summary>Reemplazo completo. NoEncontradoExcepcion · ReferenciaInvalidaExcepcion.</summary>
    Task<int> Reemplazar(int activAcademicasIdcurso, int registroCalificadoCodigo, AaRc entidad);

    /// <summary>Escribe solo lo enviado. ArgumentException si no llegó nada.</summary>
    Task<int> ActualizarParcial(int activAcademicasIdcurso, int registroCalificadoCodigo, AaRcCambios cambios);

    /// <summary>Borrado lógico. NoEncontradoExcepcion si no existe.</summary>
    Task<int> Eliminar(int activAcademicasIdcurso, int registroCalificadoCodigo);
}
