using ApiInnovacion.Excepciones;
using ApiInnovacion.Modelos;
using ApiInnovacion.Repositorios;

namespace ApiInnovacion.Servicios;

/// <summary>
/// La capa 2: las reglas de negocio de acreditacion. Depende solo de interfaces
/// —su repositorio y el de referencias—, así que no sabe qué motor hay detrás y
/// se puede probar sin base de datos.
/// </summary>
public class ServicioAcreditacion : IServicioAcreditacion
{
    private readonly IRepositorioAcreditacion _repositorio;
    private readonly IRepositorioReferencias _referencias;

    public ServicioAcreditacion(IRepositorioAcreditacion repositorio, IRepositorioReferencias referencias)
    {
        _repositorio = repositorio;
        _referencias = referencias;
    }

    public async Task<IEnumerable<Acreditacion>> ObtenerTodos(int limite)
    {
        // La FORMA del dato es correcta (sí es un entero): esto es 400, no 422.
        if (limite <= 0)
        {
            throw new ArgumentException("El parámetro limite debe ser un número mayor a 0.");
        }

        return await _repositorio.ObtenerTodos(limite);
    }

    public async Task<Acreditacion> ObtenerPorResolucion(int resolucion)
    {
        var entidad = await _repositorio.ObtenerPorResolucion(resolucion);
        if (entidad == null)
        {
            throw new NoEncontradoExcepcion($"No existe un registro de acreditacion con resolucion {resolucion}.");
        }

        return entidad;
    }

    public async Task Crear(Acreditacion entidad)
    {
        await ValidarReglas(entidad);
        await _repositorio.Crear(entidad);
    }

    public async Task<int> Reemplazar(int resolucion, Acreditacion entidad)
    {
        // La llave identifica la fila y viene de la ruta, no del cuerpo.
        entidad.Resolucion = resolucion;

        await ValidarReglas(entidad);

        var filasAfectadas = await _repositorio.Reemplazar(entidad);
        if (filasAfectadas == 0)
        {
            throw new NoEncontradoExcepcion($"No existe un registro de acreditacion con resolucion {resolucion}.");
        }

        return filasAfectadas;
    }

    public async Task<int> ActualizarParcial(int resolucion, AcreditacionCambios cambios)
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
        var actual = await ObtenerPorResolucion(resolucion);
        if (cambios.Tipo != null) actual.Tipo = cambios.Tipo;
        if (cambios.Calificacion != null) actual.Calificacion = cambios.Calificacion;
        if (cambios.FechaInicio != null) actual.FechaInicio = cambios.FechaInicio;
        if (cambios.FechaFin != null) actual.FechaFin = cambios.FechaFin;
        if (cambios.Programa != null) actual.Programa = cambios.Programa.Value;
        await ValidarReglas(actual);

        var filasAfectadas = await _repositorio.ActualizarParcial(resolucion, cambios);
        if (filasAfectadas == 0)
        {
            throw new NoEncontradoExcepcion($"No existe un registro de acreditacion con resolucion {resolucion}.");
        }

        return filasAfectadas;
    }

    public async Task<int> Eliminar(int resolucion)
    {
        var filasAfectadas = await _repositorio.EliminarLogico(resolucion);
        if (filasAfectadas == 0)
        {
            throw new NoEncontradoExcepcion($"No existe un registro de acreditacion con resolucion {resolucion}.");
        }

        return filasAfectadas;
    }

    /// <summary>
    /// Las reglas que el motor NO puede defender solo: que cada clave foránea
    /// apunte a un registro ACTIVO (la FK del motor acepta uno retirado) y las
    /// reglas propias de acreditacion.
    /// </summary>
    private async Task ValidarReglas(Acreditacion entidad)
    {
        if (!await _referencias.ProgramaActivo(entidad.Programa))
        {
            throw new ReferenciaInvalidaExcepcion(
                $"El programa {entidad.Programa} no existe o está retirado: el campo programa debe apuntar a un registro activo.");
        }
    }
}
