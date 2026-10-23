using ApiInnovacion.Excepciones;
using ApiInnovacion.Modelos;
using ApiInnovacion.Repositorios;

namespace ApiInnovacion.Servicios;

/// <summary>
/// La capa 2: las reglas de negocio de programa_ac. Depende solo de interfaces
/// —su repositorio y el de referencias—, así que no sabe qué motor hay detrás y
/// se puede probar sin base de datos.
/// </summary>
public class ServicioProgramaAc : IServicioProgramaAc
{
    private readonly IRepositorioProgramaAc _repositorio;
    private readonly IRepositorioReferencias _referencias;

    public ServicioProgramaAc(IRepositorioProgramaAc repositorio, IRepositorioReferencias referencias)
    {
        _repositorio = repositorio;
        _referencias = referencias;
    }

    public async Task<IEnumerable<ProgramaAc>> ObtenerTodos(int limite)
    {
        // La FORMA del dato es correcta (sí es un entero): esto es 400, no 422.
        if (limite <= 0)
        {
            throw new ArgumentException("El parámetro limite debe ser un número mayor a 0.");
        }

        return await _repositorio.ObtenerTodos(limite);
    }

    public async Task<ProgramaAc> ObtenerPorLlave(int programa, string areaConocimiento)
    {
        var entidad = await _repositorio.ObtenerPorLlave(programa, areaConocimiento);
        if (entidad == null)
        {
            throw new NoEncontradoExcepcion($"No existe un registro de programa_ac con programa {programa}, area_conocimiento {areaConocimiento}.");
        }

        return entidad;
    }

    public async Task Crear(ProgramaAc entidad)
    {
        await ValidarReglas(entidad);
        await _repositorio.Crear(entidad);
    }

    public async Task<int> Eliminar(int programa, string areaConocimiento)
    {
        var filasAfectadas = await _repositorio.EliminarLogico(programa, areaConocimiento);
        if (filasAfectadas == 0)
        {
            throw new NoEncontradoExcepcion($"No existe un registro de programa_ac con programa {programa}, area_conocimiento {areaConocimiento}.");
        }

        return filasAfectadas;
    }

    /// <summary>
    /// Las reglas que el motor NO puede defender solo: que cada clave foránea
    /// apunte a un registro ACTIVO (la FK del motor acepta uno retirado) y las
    /// reglas propias de programa_ac.
    /// </summary>
    private async Task ValidarReglas(ProgramaAc entidad)
    {
        if (!await _referencias.ProgramaActivo(entidad.Programa))
        {
            throw new ReferenciaInvalidaExcepcion(
                $"El programa {entidad.Programa} no existe o está retirado: el campo programa debe apuntar a un registro activo.");
        }

        if (!await _referencias.AreaConocimientoActiva(entidad.AreaConocimiento))
        {
            throw new ReferenciaInvalidaExcepcion(
                $"El área de conocimiento {entidad.AreaConocimiento} no existe o está retirado: el campo areaConocimiento debe apuntar a un registro activo.");
        }
    }
}
