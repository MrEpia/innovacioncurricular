using ApiInnovacion.Modelos;

namespace ApiInnovacion.Repositorios;

/// <summary>
/// El contrato de la capa de datos para aa_rc. El servicio conoce ESTA
/// interfaz y nada más.
/// </summary>
public interface IRepositorioAaRc
{
    Task<IEnumerable<AaRc>> ObtenerTodos(int limite);
    Task<AaRc?> ObtenerPorLlave(int activAcademicasIdcurso, int registroCalificadoCodigo);
    Task Crear(AaRc entidad);
    Task<int> Reemplazar(AaRc entidad);
    Task<int> ActualizarParcial(int activAcademicasIdcurso, int registroCalificadoCodigo, AaRcCambios cambios);
    Task<int> EliminarLogico(int activAcademicasIdcurso, int registroCalificadoCodigo);
}
