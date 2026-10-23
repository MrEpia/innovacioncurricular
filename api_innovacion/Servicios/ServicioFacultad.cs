using ApiInnovacion.Excepciones;
using ApiInnovacion.Modelos;
using ApiInnovacion.Repositorios;

namespace ApiInnovacion.Servicios;

/// <summary>
/// La capa 2: las reglas de negocio de facultad. Depende solo de interfaces
/// —su repositorio y el de referencias—, así que no sabe qué motor hay detrás y
/// se puede probar sin base de datos.
/// </summary>
public class ServicioFacultad : IServicioFacultad
{
    private readonly IRepositorioFacultad _repositorio;
    private readonly IRepositorioReferencias _referencias;

    private static readonly string[] TiposValidos = { "Facultad", "Departamento", "Instituto" };

    public ServicioFacultad(IRepositorioFacultad repositorio, IRepositorioReferencias referencias)
    {
        _repositorio = repositorio;
        _referencias = referencias;
    }

    public async Task<IEnumerable<Facultad>> ObtenerTodos(int limite)
    {
        // La FORMA del dato es correcta (sí es un entero): esto es 400, no 422.
        if (limite <= 0)
        {
            throw new ArgumentException("El parámetro limite debe ser un número mayor a 0.");
        }

        return await _repositorio.ObtenerTodos(limite);
    }

    public async Task<Facultad> ObtenerPorId(int id)
    {
        var entidad = await _repositorio.ObtenerPorId(id);
        if (entidad == null)
        {
            throw new NoEncontradoExcepcion($"No existe un registro de facultad con id {id}.");
        }

        return entidad;
    }

    public async Task Crear(Facultad entidad)
    {
        await ValidarReglas(entidad);
        await _repositorio.Crear(entidad);
    }

    public async Task<int> Reemplazar(int id, Facultad entidad)
    {
        // La llave identifica la fila y viene de la ruta, no del cuerpo.
        entidad.Id = id;

        await ValidarReglas(entidad);

        var filasAfectadas = await _repositorio.Reemplazar(entidad);
        if (filasAfectadas == 0)
        {
            throw new NoEncontradoExcepcion($"No existe un registro de facultad con id {id}.");
        }

        return filasAfectadas;
    }

    public async Task<int> ActualizarParcial(int id, FacultadCambios cambios)
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
        if (cambios.FechaFun != null) actual.FechaFun = cambios.FechaFun.Value;
        if (cambios.Universidad != null) actual.Universidad = cambios.Universidad.Value;
        await ValidarReglas(actual);

        var filasAfectadas = await _repositorio.ActualizarParcial(id, cambios);
        if (filasAfectadas == 0)
        {
            throw new NoEncontradoExcepcion($"No existe un registro de facultad con id {id}.");
        }

        return filasAfectadas;
    }

    public async Task<int> Eliminar(int id)
    {
        // Retirarlo con hijos activos los dejaría huérfanos: seguirían apuntando a
        // un registro que ya no aparece en ninguna lista.
        var dependientes = await _referencias.DependientesDeFacultad(id);
        if (dependientes.Count > 0)
        {
            throw new DependenciasActivasExcepcion(
                $"No se puede retirar la facultad {id}: tiene registros activos en {string.Join(", ", dependientes)}. Retírelos primero.");
        }

        var filasAfectadas = await _repositorio.EliminarLogico(id);
        if (filasAfectadas == 0)
        {
            throw new NoEncontradoExcepcion($"No existe un registro de facultad con id {id}.");
        }

        return filasAfectadas;
    }

    /// <summary>
    /// Las reglas que el motor NO puede defender solo: que cada clave foránea
    /// apunte a un registro ACTIVO (la FK del motor acepta uno retirado) y las
    /// reglas propias de facultad.
    /// </summary>
    private async Task ValidarReglas(Facultad entidad)
    {
        if (!await _referencias.UniversidadActiva(entidad.Universidad))
        {
            throw new ReferenciaInvalidaExcepcion(
                $"La universidad {entidad.Universidad} no existe o está retirada: el campo universidad debe apuntar a un registro activo.");
        }

        // El módulo define tres tipos de unidad académica y ninguno más.
        if (!TiposValidos.Contains(entidad.Tipo))
        {
            throw new ArgumentException(
                $"El tipo debe ser uno de: {string.Join(", ", TiposValidos)}.");
        }
    }
}
