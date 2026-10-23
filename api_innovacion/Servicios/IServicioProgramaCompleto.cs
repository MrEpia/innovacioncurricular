using ApiInnovacion.Modelos;

namespace ApiInnovacion.Servicios;

/// <summary>Las reglas del maestro-detalle de programa.</summary>
public interface IServicioProgramaCompleto
{
    Task Crear(ProgramaCompleto entidad);

    Task<ProgramaCompleto> ObtenerCompleto(int id);
}
