using ApiInnovacion.Excepciones;
using ApiInnovacion.Modelos;
using ApiInnovacion.Repositorios;

namespace ApiInnovacion.Servicios;

/// <summary>
/// La capa 2: las reglas de negocio de an_programa. Depende solo de interfaces
/// —su repositorio y el de referencias—, así que no sabe qué motor hay detrás y
/// se puede probar sin base de datos.
/// </summary>
public class ServicioAnPrograma : IServicioAnPrograma
{
    private readonly IRepositorioAnPrograma _repositorio;
    private readonly IRepositorioReferencias _referencias;

    public ServicioAnPrograma(IRepositorioAnPrograma repositorio, IRepositorioReferencias referencias)
    {
        _repositorio = repositorio;
        _referencias = referencias;
    }

    public async Task<IEnumerable<AnPrograma>> ObtenerTodos(int limite)
    {
        // La FORMA del dato es correcta (sí es un entero): esto es 400, no 422.
        if (limite <= 0)
        {
            throw new ArgumentException("El parámetro limite debe ser un número mayor a 0.");
        }

        return await _repositorio.ObtenerTodos(limite);
    }

    public async Task<AnPrograma> ObtenerPorLlave(int aspectoNormativo, int programa)
    {
        var entidad = await _repositorio.ObtenerPorLlave(aspectoNormativo, programa);
        if (entidad == null)
        {
            throw new NoEncontradoExcepcion($"No existe un registro de an_programa con aspecto_normativo {aspectoNormativo}, programa {programa}.");
        }

        return entidad;
    }

    public async Task Crear(AnPrograma entidad)
    {
        await ValidarReglas(entidad);
        await _repositorio.Crear(entidad);
    }

    public async Task<int> Eliminar(int aspectoNormativo, int programa)
    {
        var filasAfectadas = await _repositorio.EliminarLogico(aspectoNormativo, programa);
        if (filasAfectadas == 0)
        {
            throw new NoEncontradoExcepcion($"No existe un registro de an_programa con aspecto_normativo {aspectoNormativo}, programa {programa}.");
        }

        return filasAfectadas;
    }

    /// <summary>
    /// Las reglas que el motor NO puede defender solo: que cada clave foránea
    /// apunte a un registro ACTIVO (la FK del motor acepta uno retirado) y las
    /// reglas propias de an_programa.
    /// </summary>
    private async Task ValidarReglas(AnPrograma entidad)
    {
        if (!await _referencias.AspectoNormativoActivo(entidad.AspectoNormativo))
        {
            throw new ReferenciaInvalidaExcepcion(
                $"El aspecto normativo {entidad.AspectoNormativo} no existe o está retirado: el campo aspectoNormativo debe apuntar a un registro activo.");
        }

        if (!await _referencias.ProgramaActivo(entidad.Programa))
        {
            throw new ReferenciaInvalidaExcepcion(
                $"El programa {entidad.Programa} no existe o está retirado: el campo programa debe apuntar a un registro activo.");
        }
    }
}
