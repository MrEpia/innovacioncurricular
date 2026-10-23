using ApiInnovacion.Excepciones;
using ApiInnovacion.Modelos;
using ApiInnovacion.Repositorios;

namespace ApiInnovacion.Servicios;

/// <summary>
/// La capa 2: las reglas de negocio de programa_pe. Depende solo de interfaces
/// —su repositorio y el de referencias—, así que no sabe qué motor hay detrás y
/// se puede probar sin base de datos.
/// </summary>
public class ServicioProgramaPe : IServicioProgramaPe
{
    private readonly IRepositorioProgramaPe _repositorio;
    private readonly IRepositorioReferencias _referencias;

    public ServicioProgramaPe(IRepositorioProgramaPe repositorio, IRepositorioReferencias referencias)
    {
        _repositorio = repositorio;
        _referencias = referencias;
    }

    public async Task<IEnumerable<ProgramaPe>> ObtenerTodos(int limite)
    {
        // La FORMA del dato es correcta (sí es un entero): esto es 400, no 422.
        if (limite <= 0)
        {
            throw new ArgumentException("El parámetro limite debe ser un número mayor a 0.");
        }

        return await _repositorio.ObtenerTodos(limite);
    }

    public async Task<ProgramaPe> ObtenerPorLlave(int programa, int practicaEstrategia)
    {
        var entidad = await _repositorio.ObtenerPorLlave(programa, practicaEstrategia);
        if (entidad == null)
        {
            throw new NoEncontradoExcepcion($"No existe un registro de programa_pe con programa {programa}, practica_estrategia {practicaEstrategia}.");
        }

        return entidad;
    }

    public async Task Crear(ProgramaPe entidad)
    {
        await ValidarReglas(entidad);
        await _repositorio.Crear(entidad);
    }

    public async Task<int> Eliminar(int programa, int practicaEstrategia)
    {
        var filasAfectadas = await _repositorio.EliminarLogico(programa, practicaEstrategia);
        if (filasAfectadas == 0)
        {
            throw new NoEncontradoExcepcion($"No existe un registro de programa_pe con programa {programa}, practica_estrategia {practicaEstrategia}.");
        }

        return filasAfectadas;
    }

    /// <summary>
    /// Las reglas que el motor NO puede defender solo: que cada clave foránea
    /// apunte a un registro ACTIVO (la FK del motor acepta uno retirado) y las
    /// reglas propias de programa_pe.
    /// </summary>
    private async Task ValidarReglas(ProgramaPe entidad)
    {
        if (!await _referencias.ProgramaActivo(entidad.Programa))
        {
            throw new ReferenciaInvalidaExcepcion(
                $"El programa {entidad.Programa} no existe o está retirado: el campo programa debe apuntar a un registro activo.");
        }

        if (!await _referencias.PracticaEstrategiaActiva(entidad.PracticaEstrategia))
        {
            throw new ReferenciaInvalidaExcepcion(
                $"La práctica o estrategia {entidad.PracticaEstrategia} no existe o está retirada: el campo practicaEstrategia debe apuntar a un registro activo.");
        }
    }
}
