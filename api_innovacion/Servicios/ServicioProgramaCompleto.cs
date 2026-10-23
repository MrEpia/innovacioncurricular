using ApiInnovacion.Excepciones;
using ApiInnovacion.Modelos;
using ApiInnovacion.Repositorios;

namespace ApiInnovacion.Servicios;

/// <summary>
/// La capa 2 del maestro-detalle de programa. Valida el maestro y cada hijo
/// antes de escribir; el todo-o-nada lo garantiza el procedimiento del
/// repositorio.
/// </summary>
public class ServicioProgramaCompleto : IServicioProgramaCompleto
{
    private readonly IRepositorioProgramaCompleto _repositorio;
    private readonly IRepositorioReferencias _referencias;

    public ServicioProgramaCompleto(IRepositorioProgramaCompleto repositorio, IRepositorioReferencias referencias)
    {
        _repositorio = repositorio;
        _referencias = referencias;
    }

    public async Task Crear(ProgramaCompleto entidad)
    {
        if (entidad.AreasConocimiento.Count == 0 && entidad.PracticasEstrategias.Count == 0
            && entidad.CaracteristicasInnovacion.Count == 0)
        {
            throw new ArgumentException(
                "El detalle está vacío: envíe al menos un área, una práctica o una característica. Para crear solo el programa use POST /api/programa.");
        }

        // --- el maestro: las mismas reglas que ServicioPrograma
        if (!await _referencias.FacultadActiva(entidad.Facultad))
        {
            throw new ReferenciaInvalidaExcepcion(
                $"La facultad {entidad.Facultad} no existe o está retirada: el campo facultad debe apuntar a un registro activo.");
        }

        // --- el detalle: sin repetidos...
        Repetido(entidad.AreasConocimiento.Select(a => a.AreaConocimiento), "El área de conocimiento");
        Repetido(entidad.PracticasEstrategias.Select(p => p.PracticaEstrategia), "La práctica o estrategia");
        Repetido(entidad.CaracteristicasInnovacion.Select(c => c.CarInnovacion), "La característica de innovación");

        // ...y cada hijo apuntando a un registro ACTIVO.
        foreach (var area in entidad.AreasConocimiento)
        {
            if (!await _referencias.AreaConocimientoActiva(area.AreaConocimiento))
            {
                throw new ReferenciaInvalidaExcepcion($"El área de conocimiento {area.AreaConocimiento} no existe o está retirada.");
            }
        }

        foreach (var practica in entidad.PracticasEstrategias)
        {
            if (!await _referencias.PracticaEstrategiaActiva(practica.PracticaEstrategia))
            {
                throw new ReferenciaInvalidaExcepcion($"La práctica o estrategia {practica.PracticaEstrategia} no existe o está retirada.");
            }
        }

        foreach (var caracteristica in entidad.CaracteristicasInnovacion)
        {
            if (!await _referencias.CarInnovacionActiva(caracteristica.CarInnovacion))
            {
                throw new ReferenciaInvalidaExcepcion($"La característica de innovación {caracteristica.CarInnovacion} no existe o está retirada.");
            }
        }

        await _repositorio.Crear(entidad);
    }

    public async Task<ProgramaCompleto> ObtenerCompleto(int id)
    {
        return await _repositorio.ObtenerCompleto(id)
            ?? throw new NoEncontradoExcepcion($"No existe un registro de programa con id {id}.");
    }

    private static void Repetido<T>(IEnumerable<T> llaves, string que)
    {
        var repetida = llaves.GroupBy(x => x).FirstOrDefault(g => g.Count() > 1);
        if (repetida != null)
        {
            throw new ArgumentException($"{que} {repetida.Key} está repetida en la lista.");
        }
    }
}
