using ApiInnovacion.Modelos;

namespace ApiInnovacion.Repositorios;

/// <summary>La capa 3 del maestro-detalle de programa.</summary>
public interface IRepositorioProgramaCompleto
{
    /// <summary>Guarda el programa, sus programa_ac, programa_pe y programa_ci en una transacción.</summary>
    Task Crear(ProgramaCompleto entidad);

    /// <summary>El programa activo con su detalle activo, o null.</summary>
    Task<ProgramaCompleto?> ObtenerCompleto(int id);
}
