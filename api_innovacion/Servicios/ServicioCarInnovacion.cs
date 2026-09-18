using ApiInnovacion.Excepciones;
using ApiInnovacion.Modelos;
using ApiInnovacion.Repositorios;

namespace ApiInnovacion.Servicios;

/// <summary>
/// La capa 2: las reglas de negocio para car_innovacion. Depende solo de la interfaz
/// del repositorio, así que no sabe qué motor hay detrás — y por eso se puede
/// probar sin base de datos.
/// </summary>
public class ServicioCarInnovacion : IServicioCarInnovacion
{
    private readonly IRepositorioCarInnovacion _repositorio;

    public ServicioCarInnovacion(IRepositorioCarInnovacion repositorio)
    {
        _repositorio = repositorio;
    }

    public async Task<IEnumerable<CarInnovacion>> ObtenerTodos(int limite)
    {
        // Regla de negocio: el límite debe ser positivo. La FORMA del dato es
        // correcta (sí es un entero), así que esto es 400 y no 422.
        if (limite <= 0)
        {
            throw new ArgumentException("El parámetro limite debe ser un número mayor a 0.");
        }

        return await _repositorio.ObtenerTodos(limite);
    }

    public async Task<CarInnovacion> ObtenerPorId(int id)
    {
        var entidad = await _repositorio.ObtenerPorId(id);
        if (entidad == null)
        {
            throw new NoEncontradoExcepcion($"No existe un registro de car_innovacion con id {id}.");
        }

        return entidad;
    }

    public async Task Crear(CarInnovacion entidad)
    {
        await _repositorio.Crear(entidad);
    }

    public async Task<int> Reemplazar(int id, CarInnovacion entidad)
    {
        // La llave identifica la fila y viene de la ruta, no del cuerpo
        entidad.Id = id;

        var filasAfectadas = await _repositorio.Reemplazar(entidad);
        if (filasAfectadas == 0)
        {
            throw new NoEncontradoExcepcion($"No existe un registro de car_innovacion con id {id}.");
        }

        return filasAfectadas;
    }

    public async Task<int> ActualizarParcial(int id, string? nombre, string? descripcion, string? tipo)
    {
        // Sin esta comprobación el repositorio devolvería 0 filas, que en toda
        // la demás lógica significa "no existe" — y responderíamos 404 en vez
        // del 400 que exige el contrato para un cuerpo vacío.
        if (nombre == null && descripcion == null && tipo == null)
        {
            throw new ArgumentException("No se envió ningún campo para actualizar.");
        }

        var filasAfectadas = await _repositorio.ActualizarParcial(
            id, nombre, descripcion, tipo);

        if (filasAfectadas == 0)
        {
            throw new NoEncontradoExcepcion($"No existe un registro de car_innovacion con id {id}.");
        }

        return filasAfectadas;
    }

    public async Task<int> Eliminar(int id)
    {
        var filasAfectadas = await _repositorio.EliminarLogico(id);
        if (filasAfectadas == 0)
        {
            throw new NoEncontradoExcepcion($"No existe un registro de car_innovacion con id {id}.");
        }

        return filasAfectadas;
    }
}
