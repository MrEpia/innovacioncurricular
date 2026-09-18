using ApiInnovacion.Excepciones;
using ApiInnovacion.Modelos;
using ApiInnovacion.Repositorios;

namespace ApiInnovacion.Servicios;

/// <summary>
/// La capa 2: las reglas de negocio para area_conocimiento. Depende solo de la interfaz
/// del repositorio, así que no sabe qué motor hay detrás — y por eso se puede
/// probar sin base de datos.
/// </summary>
public class ServicioAreaConocimiento : IServicioAreaConocimiento
{
    private readonly IRepositorioAreaConocimiento _repositorio;

    public ServicioAreaConocimiento(IRepositorioAreaConocimiento repositorio)
    {
        _repositorio = repositorio;
    }

    public async Task<IEnumerable<AreaConocimiento>> ObtenerTodos(int limite)
    {
        // Regla de negocio: el límite debe ser positivo. La FORMA del dato es
        // correcta (sí es un entero), así que esto es 400 y no 422.
        if (limite <= 0)
        {
            throw new ArgumentException("El parámetro limite debe ser un número mayor a 0.");
        }

        return await _repositorio.ObtenerTodos(limite);
    }

    public async Task<AreaConocimiento> ObtenerPorId(string id)
    {
        var entidad = await _repositorio.ObtenerPorId(id);
        if (entidad == null)
        {
            throw new NoEncontradoExcepcion($"No existe un registro de area_conocimiento con id {id}.");
        }

        return entidad;
    }

    public async Task Crear(AreaConocimiento entidad)
    {
        await _repositorio.Crear(entidad);
    }

    public async Task<int> Reemplazar(string id, AreaConocimiento entidad)
    {
        // La llave identifica la fila y viene de la ruta, no del cuerpo
        entidad.Id = id;

        var filasAfectadas = await _repositorio.Reemplazar(entidad);
        if (filasAfectadas == 0)
        {
            throw new NoEncontradoExcepcion($"No existe un registro de area_conocimiento con id {id}.");
        }

        return filasAfectadas;
    }

    public async Task<int> ActualizarParcial(string id, string? granArea, string? area, string? disciplina)
    {
        // Sin esta comprobación el repositorio devolvería 0 filas, que en toda
        // la demás lógica significa "no existe" — y responderíamos 404 en vez
        // del 400 que exige el contrato para un cuerpo vacío.
        if (granArea == null && area == null && disciplina == null)
        {
            throw new ArgumentException("No se envió ningún campo para actualizar.");
        }

        var filasAfectadas = await _repositorio.ActualizarParcial(
            id, granArea, area, disciplina);

        if (filasAfectadas == 0)
        {
            throw new NoEncontradoExcepcion($"No existe un registro de area_conocimiento con id {id}.");
        }

        return filasAfectadas;
    }

    public async Task<int> Eliminar(string id)
    {
        var filasAfectadas = await _repositorio.EliminarLogico(id);
        if (filasAfectadas == 0)
        {
            throw new NoEncontradoExcepcion($"No existe un registro de area_conocimiento con id {id}.");
        }

        return filasAfectadas;
    }
}
