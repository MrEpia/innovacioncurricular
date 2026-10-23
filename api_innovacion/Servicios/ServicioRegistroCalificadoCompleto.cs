using ApiInnovacion.Excepciones;
using ApiInnovacion.Modelos;
using ApiInnovacion.Repositorios;

namespace ApiInnovacion.Servicios;

/// <summary>
/// La capa 2 del maestro-detalle de registro_calificado.
///
/// Valida TODO antes de escribir nada —el maestro, cada hijo y que no haya
/// hijos repetidos— para responder el primer error con un mensaje claro. La
/// atomicidad (todo o nada) la garantiza el procedimiento que llama el
/// repositorio; esta capa no la sabe ni la necesita, y por eso se prueba sin
/// base de datos.
/// </summary>
public class ServicioRegistroCalificadoCompleto : IServicioRegistroCalificadoCompleto
{
    private readonly IRepositorioRegistroCalificadoCompleto _repositorio;
    private readonly IRepositorioReferencias _referencias;

    public ServicioRegistroCalificadoCompleto(IRepositorioRegistroCalificadoCompleto repositorio, IRepositorioReferencias referencias)
    {
        _repositorio = repositorio;
        _referencias = referencias;
    }

    public async Task Crear(RegistroCalificadoCompleto entidad)
    {
        // Sin detalle no es un maestro-detalle: para eso está POST /api/registro_calificado.
        if (entidad.Enfoques.Count == 0 && entidad.Actividades.Count == 0)
        {
            throw new ArgumentException(
                "El detalle está vacío: envíe al menos un enfoque o una actividad. Para crear solo el registro use POST /api/registro_calificado.");
        }

        // --- el maestro: las mismas reglas que ServicioRegistroCalificado
        if (!await _referencias.ProgramaActivo(entidad.Programa))
        {
            throw new ReferenciaInvalidaExcepcion(
                $"El programa {entidad.Programa} no existe o está retirado: el campo programa debe apuntar a un registro activo.");
        }

        if (entidad.FechaFin < entidad.FechaInicio)
        {
            throw new ArgumentException("El campo fechaFin no puede ser anterior a fechaInicio.");
        }

        // --- el detalle: sin repetidos (serían la misma llave dos veces)...
        var enfoqueRepetido = entidad.Enfoques.GroupBy(e => e.Enfoque).FirstOrDefault(g => g.Count() > 1);
        if (enfoqueRepetido != null)
        {
            throw new ArgumentException($"El enfoque {enfoqueRepetido.Key} está repetido en la lista.");
        }

        var actividadRepetida = entidad.Actividades.GroupBy(a => a.ActivAcademicasIdcurso).FirstOrDefault(g => g.Count() > 1);
        if (actividadRepetida != null)
        {
            throw new ArgumentException($"La actividad académica {actividadRepetida.Key} está repetida en la lista.");
        }

        // ...y cada hijo apuntando a un registro ACTIVO.
        foreach (var enfoque in entidad.Enfoques)
        {
            if (!await _referencias.EnfoqueActivo(enfoque.Enfoque))
            {
                throw new ReferenciaInvalidaExcepcion($"El enfoque {enfoque.Enfoque} no existe o está retirado.");
            }
        }

        foreach (var actividad in entidad.Actividades)
        {
            if (!await _referencias.ActivAcademicaActiva(actividad.ActivAcademicasIdcurso))
            {
                throw new ReferenciaInvalidaExcepcion(
                    $"La actividad académica {actividad.ActivAcademicasIdcurso} no existe o está retirada.");
            }
        }

        await _repositorio.Crear(entidad);
    }

    public async Task<RegistroCalificadoCompleto> ObtenerCompleto(int codigo)
    {
        return await _repositorio.ObtenerCompleto(codigo)
            ?? throw new NoEncontradoExcepcion($"No existe un registro de registro_calificado con codigo {codigo}.");
    }
}
