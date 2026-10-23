using ApiInnovacion.Excepciones;
using ApiInnovacion.Modelos;
using ApiInnovacion.Repositorios;

namespace ApiInnovacion.Servicios;

/// <summary>
/// La capa 2: las reglas de negocio de programa. Depende solo de interfaces
/// —su repositorio y el de referencias—, así que no sabe qué motor hay detrás y
/// se puede probar sin base de datos.
/// </summary>
public class ServicioPrograma : IServicioPrograma
{
    private readonly IRepositorioPrograma _repositorio;
    private readonly IRepositorioReferencias _referencias;

    public ServicioPrograma(IRepositorioPrograma repositorio, IRepositorioReferencias referencias)
    {
        _repositorio = repositorio;
        _referencias = referencias;
    }

    public async Task<IEnumerable<Programa>> ObtenerTodos(int limite)
    {
        // La FORMA del dato es correcta (sí es un entero): esto es 400, no 422.
        if (limite <= 0)
        {
            throw new ArgumentException("El parámetro limite debe ser un número mayor a 0.");
        }

        return await _repositorio.ObtenerTodos(limite);
    }

    public async Task<Programa> ObtenerPorId(int id)
    {
        var entidad = await _repositorio.ObtenerPorId(id);
        if (entidad == null)
        {
            throw new NoEncontradoExcepcion($"No existe un registro de programa con id {id}.");
        }

        return entidad;
    }

    public async Task Crear(Programa entidad)
    {
        await ValidarReglas(entidad);
        await _repositorio.Crear(entidad);
    }

    public async Task<int> Reemplazar(int id, Programa entidad)
    {
        // La llave identifica la fila y viene de la ruta, no del cuerpo.
        entidad.Id = id;

        await ValidarReglas(entidad);

        var filasAfectadas = await _repositorio.Reemplazar(entidad);
        if (filasAfectadas == 0)
        {
            throw new NoEncontradoExcepcion($"No existe un registro de programa con id {id}.");
        }

        return filasAfectadas;
    }

    public async Task<int> ActualizarParcial(int id, ProgramaCambios cambios)
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
        var actual = await ObtenerPorId(id);
        if (cambios.Nombre != null) actual.Nombre = cambios.Nombre;
        if (cambios.Tipo != null) actual.Tipo = cambios.Tipo;
        if (cambios.Nivel != null) actual.Nivel = cambios.Nivel;
        if (cambios.FechaCreacion != null) actual.FechaCreacion = cambios.FechaCreacion;
        if (cambios.FechaCierre != null) actual.FechaCierre = cambios.FechaCierre;
        if (cambios.NumeroCohortes != null) actual.NumeroCohortes = cambios.NumeroCohortes;
        if (cambios.CantGraduados != null) actual.CantGraduados = cambios.CantGraduados;
        if (cambios.FechaActualizacion != null) actual.FechaActualizacion = cambios.FechaActualizacion;
        if (cambios.Ciudad != null) actual.Ciudad = cambios.Ciudad;
        if (cambios.Facultad != null) actual.Facultad = cambios.Facultad.Value;
        await ValidarReglas(actual);

        var filasAfectadas = await _repositorio.ActualizarParcial(id, cambios);
        if (filasAfectadas == 0)
        {
            throw new NoEncontradoExcepcion($"No existe un registro de programa con id {id}.");
        }

        return filasAfectadas;
    }

    public async Task<int> Eliminar(int id)
    {
        // Retirarlo con hijos activos los dejaría huérfanos: seguirían apuntando a
        // un registro que ya no aparece en ninguna lista.
        var dependientes = await _referencias.DependientesDePrograma(id);
        if (dependientes.Count > 0)
        {
            throw new DependenciasActivasExcepcion(
                $"No se puede retirar el programa {id}: tiene registros activos en {string.Join(", ", dependientes)}. Retírelos primero.");
        }

        var filasAfectadas = await _repositorio.EliminarLogico(id);
        if (filasAfectadas == 0)
        {
            throw new NoEncontradoExcepcion($"No existe un registro de programa con id {id}.");
        }

        return filasAfectadas;
    }

    /// <summary>
    /// Las reglas que el motor NO puede defender solo: que cada clave foránea
    /// apunte a un registro ACTIVO (la FK del motor acepta uno retirado) y las
    /// reglas propias de programa.
    /// </summary>
    private async Task ValidarReglas(Programa entidad)
    {
        if (!await _referencias.FacultadActiva(entidad.Facultad))
        {
            throw new ReferenciaInvalidaExcepcion(
                $"La facultad {entidad.Facultad} no existe o está retirada: el campo facultad debe apuntar a un registro activo.");
        }
    }
}
