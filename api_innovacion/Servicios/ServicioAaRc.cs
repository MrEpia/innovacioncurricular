using ApiInnovacion.Excepciones;
using ApiInnovacion.Modelos;
using ApiInnovacion.Repositorios;

namespace ApiInnovacion.Servicios;

/// <summary>
/// La capa 2: las reglas de negocio de aa_rc. Depende solo de interfaces
/// —su repositorio y el de referencias—, así que no sabe qué motor hay detrás y
/// se puede probar sin base de datos.
/// </summary>
public class ServicioAaRc : IServicioAaRc
{
    private readonly IRepositorioAaRc _repositorio;
    private readonly IRepositorioReferencias _referencias;

    public ServicioAaRc(IRepositorioAaRc repositorio, IRepositorioReferencias referencias)
    {
        _repositorio = repositorio;
        _referencias = referencias;
    }

    public async Task<IEnumerable<AaRc>> ObtenerTodos(int limite)
    {
        // La FORMA del dato es correcta (sí es un entero): esto es 400, no 422.
        if (limite <= 0)
        {
            throw new ArgumentException("El parámetro limite debe ser un número mayor a 0.");
        }

        return await _repositorio.ObtenerTodos(limite);
    }

    public async Task<AaRc> ObtenerPorLlave(int activAcademicasIdcurso, int registroCalificadoCodigo)
    {
        var entidad = await _repositorio.ObtenerPorLlave(activAcademicasIdcurso, registroCalificadoCodigo);
        if (entidad == null)
        {
            throw new NoEncontradoExcepcion($"No existe un registro de aa_rc con activ_academicas_idcurso {activAcademicasIdcurso}, registro_calificado_codigo {registroCalificadoCodigo}.");
        }

        return entidad;
    }

    public async Task Crear(AaRc entidad)
    {
        await ValidarReglas(entidad);
        await _repositorio.Crear(entidad);
    }

    public async Task<int> Reemplazar(int activAcademicasIdcurso, int registroCalificadoCodigo, AaRc entidad)
    {
        // La llave identifica la fila y viene de la ruta, no del cuerpo.
        entidad.ActivAcademicasIdcurso = activAcademicasIdcurso;
        entidad.RegistroCalificadoCodigo = registroCalificadoCodigo;

        await ValidarReglas(entidad);

        var filasAfectadas = await _repositorio.Reemplazar(entidad);
        if (filasAfectadas == 0)
        {
            throw new NoEncontradoExcepcion($"No existe un registro de aa_rc con activ_academicas_idcurso {activAcademicasIdcurso}, registro_calificado_codigo {registroCalificadoCodigo}.");
        }

        return filasAfectadas;
    }

    public async Task<int> ActualizarParcial(int activAcademicasIdcurso, int registroCalificadoCodigo, AaRcCambios cambios)
    {
        // Sin esto, un cuerpo vacío llegaría al repositorio, afectaría 0 filas y
        // responderíamos 404 en vez del 400 que pide el contrato.
        if (!cambios.HayAlgo())
        {
            throw new ArgumentException("No se envió ningún campo para actualizar.");
        }

        // Las reglas se validan sobre cómo QUEDARÍA el registro: una fecha de fin
        // que llega sola tiene que compararse con la de inicio que ya estaba.
        // Leer primero también da el 404 antes de cualquier otra comprobación.
        var actual = await ObtenerPorLlave(activAcademicasIdcurso, registroCalificadoCodigo);
        if (cambios.Componente != null) actual.Componente = cambios.Componente;
        if (cambios.Semestre != null) actual.Semestre = cambios.Semestre;
        await ValidarReglas(actual);

        var filasAfectadas = await _repositorio.ActualizarParcial(activAcademicasIdcurso, registroCalificadoCodigo, cambios);
        if (filasAfectadas == 0)
        {
            throw new NoEncontradoExcepcion($"No existe un registro de aa_rc con activ_academicas_idcurso {activAcademicasIdcurso}, registro_calificado_codigo {registroCalificadoCodigo}.");
        }

        return filasAfectadas;
    }

    public async Task<int> Eliminar(int activAcademicasIdcurso, int registroCalificadoCodigo)
    {
        var filasAfectadas = await _repositorio.EliminarLogico(activAcademicasIdcurso, registroCalificadoCodigo);
        if (filasAfectadas == 0)
        {
            throw new NoEncontradoExcepcion($"No existe un registro de aa_rc con activ_academicas_idcurso {activAcademicasIdcurso}, registro_calificado_codigo {registroCalificadoCodigo}.");
        }

        return filasAfectadas;
    }

    /// <summary>
    /// Las reglas que el motor NO puede defender solo: que cada clave foránea
    /// apunte a un registro ACTIVO (la FK del motor acepta uno retirado) y las
    /// reglas propias de aa_rc.
    /// </summary>
    private async Task ValidarReglas(AaRc entidad)
    {
        if (!await _referencias.ActivAcademicaActiva(entidad.ActivAcademicasIdcurso))
        {
            throw new ReferenciaInvalidaExcepcion(
                $"La actividad académica {entidad.ActivAcademicasIdcurso} no existe o está retirada: el campo activAcademicasIdcurso debe apuntar a un registro activo.");
        }

        if (!await _referencias.RegistroCalificadoActivo(entidad.RegistroCalificadoCodigo))
        {
            throw new ReferenciaInvalidaExcepcion(
                $"El registro calificado {entidad.RegistroCalificadoCodigo} no existe o está retirado: el campo registroCalificadoCodigo debe apuntar a un registro activo.");
        }
    }
}
