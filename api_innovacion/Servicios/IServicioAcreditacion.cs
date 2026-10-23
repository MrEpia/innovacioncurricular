using ApiInnovacion.Modelos;

namespace ApiInnovacion.Servicios;

/// <summary>
/// El contrato de la capa de negocio para acreditacion. El controlador depende
/// de esta interfaz y no sabe qué hay detrás.
/// </summary>
public interface IServicioAcreditacion
{
    /// <summary>Hasta 'limite' registros activos. ArgumentException si limite &lt;= 0.</summary>
    Task<IEnumerable<Acreditacion>> ObtenerTodos(int limite);

    /// <summary>El registro con esa llave. NoEncontradoExcepcion si no existe o está inactivo.</summary>
    Task<Acreditacion> ObtenerPorResolucion(int resolucion);

    /// <summary>Crea el registro. ReferenciaInvalidaExcepcion (400) si una clave foránea no apunta
    /// a un registro activo · LlaveDuplicadaExcepcion (409) si la llave ya existe.</summary>
    Task Crear(Acreditacion entidad);

    /// <summary>Reemplazo completo. NoEncontradoExcepcion · ReferenciaInvalidaExcepcion.</summary>
    Task<int> Reemplazar(int resolucion, Acreditacion entidad);

    /// <summary>Escribe solo lo enviado. ArgumentException si no llegó nada.</summary>
    Task<int> ActualizarParcial(int resolucion, AcreditacionCambios cambios);

    /// <summary>Borrado lógico. NoEncontradoExcepcion si no existe.</summary>
    Task<int> Eliminar(int resolucion);
}
