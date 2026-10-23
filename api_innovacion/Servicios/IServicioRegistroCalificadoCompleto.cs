using ApiInnovacion.Modelos;

namespace ApiInnovacion.Servicios;

/// <summary>Las reglas del maestro-detalle de registro_calificado.</summary>
public interface IServicioRegistroCalificadoCompleto
{
    Task Crear(RegistroCalificadoCompleto entidad);

    Task<RegistroCalificadoCompleto> ObtenerCompleto(int codigo);
}
