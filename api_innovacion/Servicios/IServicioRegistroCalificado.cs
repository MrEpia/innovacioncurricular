using ApiInnovacion.Modelos;

namespace ApiInnovacion.Servicios;

/// <summary>
/// El contrato de la capa de negocio para registro_calificado. El controlador depende
/// de esta interfaz y no sabe qué hay detrás.
/// </summary>
public interface IServicioRegistroCalificado
{
    /// <summary>Hasta 'limite' registros activos. ArgumentException si limite &lt;= 0.</summary>
    Task<IEnumerable<RegistroCalificado>> ObtenerTodos(int limite);

    /// <summary>El registro con esa llave. NoEncontradoExcepcion si no existe o está inactivo.</summary>
    Task<RegistroCalificado> ObtenerPorCodigo(int codigo);

    /// <summary>Crea el registro. ReferenciaInvalidaExcepcion (400) si una clave foránea no apunta
    /// a un registro activo · LlaveDuplicadaExcepcion (409) si la llave ya existe.</summary>
    Task Crear(RegistroCalificado entidad);

    /// <summary>Reemplazo completo. NoEncontradoExcepcion · ReferenciaInvalidaExcepcion.</summary>
    Task<int> Reemplazar(int codigo, RegistroCalificado entidad);

    /// <summary>Escribe solo lo enviado. ArgumentException si no llegó nada.</summary>
    Task<int> ActualizarParcial(int codigo, RegistroCalificadoCambios cambios);

    /// <summary>Borrado lógico. NoEncontradoExcepcion si no existe · DependenciasActivasExcepcion (409) si tiene registros activos que dependen de él.</summary>
    Task<int> Eliminar(int codigo);
}
