using ApiInnovacion.Excepciones;
using ApiInnovacion.Modelos;
using ApiInnovacion.Repositorios;

namespace ApiInnovacion.Servicios;

/// <summary>
/// La capa 2: las reglas de negocio de registro_calificado. Depende solo de interfaces
/// —su repositorio y el de referencias—, así que no sabe qué motor hay detrás y
/// se puede probar sin base de datos.
/// </summary>
public class ServicioRegistroCalificado : IServicioRegistroCalificado
{
    private readonly IRepositorioRegistroCalificado _repositorio;
    private readonly IRepositorioReferencias _referencias;

    public ServicioRegistroCalificado(IRepositorioRegistroCalificado repositorio, IRepositorioReferencias referencias)
    {
        _repositorio = repositorio;
        _referencias = referencias;
    }

    public async Task<IEnumerable<RegistroCalificado>> ObtenerTodos(int limite)
    {
        // La FORMA del dato es correcta (sí es un entero): esto es 400, no 422.
        if (limite <= 0)
        {
            throw new ArgumentException("El parámetro limite debe ser un número mayor a 0.");
        }

        return await _repositorio.ObtenerTodos(limite);
    }

    public async Task<RegistroCalificado> ObtenerPorCodigo(int codigo)
    {
        var entidad = await _repositorio.ObtenerPorCodigo(codigo);
        if (entidad == null)
        {
            throw new NoEncontradoExcepcion($"No existe un registro de registro_calificado con codigo {codigo}.");
        }

        return entidad;
    }

    public async Task Crear(RegistroCalificado entidad)
    {
        await ValidarReglas(entidad);
        await _repositorio.Crear(entidad);
    }

    public async Task<int> Reemplazar(int codigo, RegistroCalificado entidad)
    {
        // La llave identifica la fila y viene de la ruta, no del cuerpo.
        entidad.Codigo = codigo;

        await ValidarReglas(entidad);

        var filasAfectadas = await _repositorio.Reemplazar(entidad);
        if (filasAfectadas == 0)
        {
            throw new NoEncontradoExcepcion($"No existe un registro de registro_calificado con codigo {codigo}.");
        }

        return filasAfectadas;
    }

    public async Task<int> ActualizarParcial(int codigo, RegistroCalificadoCambios cambios)
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
        var actual = await ObtenerPorCodigo(codigo);
        if (cambios.CantCreditos != null) actual.CantCreditos = cambios.CantCreditos;
        if (cambios.HoraAcom != null) actual.HoraAcom = cambios.HoraAcom;
        if (cambios.HoraInd != null) actual.HoraInd = cambios.HoraInd;
        if (cambios.Metodologia != null) actual.Metodologia = cambios.Metodologia;
        if (cambios.FechaInicio != null) actual.FechaInicio = cambios.FechaInicio.Value;
        if (cambios.FechaFin != null) actual.FechaFin = cambios.FechaFin.Value;
        if (cambios.DuracionAnios != null) actual.DuracionAnios = cambios.DuracionAnios;
        if (cambios.DuracionSemestres != null) actual.DuracionSemestres = cambios.DuracionSemestres;
        if (cambios.TipoTitulacion != null) actual.TipoTitulacion = cambios.TipoTitulacion;
        if (cambios.Programa != null) actual.Programa = cambios.Programa.Value;
        await ValidarReglas(actual);

        var filasAfectadas = await _repositorio.ActualizarParcial(codigo, cambios);
        if (filasAfectadas == 0)
        {
            throw new NoEncontradoExcepcion($"No existe un registro de registro_calificado con codigo {codigo}.");
        }

        return filasAfectadas;
    }

    public async Task<int> Eliminar(int codigo)
    {
        // Retirarlo con hijos activos los dejaría huérfanos: seguirían apuntando a
        // un registro que ya no aparece en ninguna lista.
        var dependientes = await _referencias.DependientesDeRegistroCalificado(codigo);
        if (dependientes.Count > 0)
        {
            throw new DependenciasActivasExcepcion(
                $"No se puede retirar el registro calificado {codigo}: tiene registros activos en {string.Join(", ", dependientes)}. Retírelos primero.");
        }

        var filasAfectadas = await _repositorio.EliminarLogico(codigo);
        if (filasAfectadas == 0)
        {
            throw new NoEncontradoExcepcion($"No existe un registro de registro_calificado con codigo {codigo}.");
        }

        return filasAfectadas;
    }

    /// <summary>
    /// Las reglas que el motor NO puede defender solo: que cada clave foránea
    /// apunte a un registro ACTIVO (la FK del motor acepta uno retirado) y las
    /// reglas propias de registro_calificado.
    /// </summary>
    private async Task ValidarReglas(RegistroCalificado entidad)
    {
        if (!await _referencias.ProgramaActivo(entidad.Programa))
        {
            throw new ReferenciaInvalidaExcepcion(
                $"El programa {entidad.Programa} no existe o está retirado: el campo programa debe apuntar a un registro activo.");
        }

        // Una fecha de cierre anterior a la de apertura es un dato imposible: la
        // base lo aceptaría, el negocio no.
        if (entidad.FechaFin < entidad.FechaInicio)
        {
            throw new ArgumentException("El campo fechaFin no puede ser anterior a fechaInicio.");
        }
    }
}
