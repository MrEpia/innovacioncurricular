using ApiInnovacion.Excepciones;
using ApiInnovacion.Modelos;
using ApiInnovacion.Repositorios;

namespace ApiInnovacion.Servicios;

/// <summary>
/// La capa 2: las reglas de negocio de enfoque_rc. Depende solo de interfaces
/// —su repositorio y el de referencias—, así que no sabe qué motor hay detrás y
/// se puede probar sin base de datos.
/// </summary>
public class ServicioEnfoqueRc : IServicioEnfoqueRc
{
    private readonly IRepositorioEnfoqueRc _repositorio;
    private readonly IRepositorioReferencias _referencias;

    public ServicioEnfoqueRc(IRepositorioEnfoqueRc repositorio, IRepositorioReferencias referencias)
    {
        _repositorio = repositorio;
        _referencias = referencias;
    }

    public async Task<IEnumerable<EnfoqueRc>> ObtenerTodos(int limite)
    {
        // La FORMA del dato es correcta (sí es un entero): esto es 400, no 422.
        if (limite <= 0)
        {
            throw new ArgumentException("El parámetro limite debe ser un número mayor a 0.");
        }

        return await _repositorio.ObtenerTodos(limite);
    }

    public async Task<EnfoqueRc> ObtenerPorLlave(int enfoque, int registroCalificado)
    {
        var entidad = await _repositorio.ObtenerPorLlave(enfoque, registroCalificado);
        if (entidad == null)
        {
            throw new NoEncontradoExcepcion($"No existe un registro de enfoque_rc con enfoque {enfoque}, registro_calificado {registroCalificado}.");
        }

        return entidad;
    }

    public async Task Crear(EnfoqueRc entidad)
    {
        await ValidarReglas(entidad);
        await _repositorio.Crear(entidad);
    }

    public async Task<int> Eliminar(int enfoque, int registroCalificado)
    {
        var filasAfectadas = await _repositorio.EliminarLogico(enfoque, registroCalificado);
        if (filasAfectadas == 0)
        {
            throw new NoEncontradoExcepcion($"No existe un registro de enfoque_rc con enfoque {enfoque}, registro_calificado {registroCalificado}.");
        }

        return filasAfectadas;
    }

    /// <summary>
    /// Las reglas que el motor NO puede defender solo: que cada clave foránea
    /// apunte a un registro ACTIVO (la FK del motor acepta uno retirado) y las
    /// reglas propias de enfoque_rc.
    /// </summary>
    private async Task ValidarReglas(EnfoqueRc entidad)
    {
        if (!await _referencias.EnfoqueActivo(entidad.Enfoque))
        {
            throw new ReferenciaInvalidaExcepcion(
                $"El enfoque {entidad.Enfoque} no existe o está retirado: el campo enfoque debe apuntar a un registro activo.");
        }

        if (!await _referencias.RegistroCalificadoActivo(entidad.RegistroCalificado))
        {
            throw new ReferenciaInvalidaExcepcion(
                $"El registro calificado {entidad.RegistroCalificado} no existe o está retirado: el campo registroCalificado debe apuntar a un registro activo.");
        }
    }
}
