using ApiInnovacion.Excepciones;
using ApiInnovacion.Modelos;
using ApiInnovacion.Repositorios;

namespace ApiInnovacion.Servicios;

/// <summary>
/// La capa 2: las reglas de negocio para aspecto_normativo. Depende solo de la interfaz
/// del repositorio, así que no sabe qué motor hay detrás — y por eso se puede
/// probar sin base de datos.
/// </summary>
public class ServicioAspectoNormativo : IServicioAspectoNormativo
{
    private readonly IRepositorioAspectoNormativo _repositorio;

    // v2: desde que existen tablas que apuntan aquí (programas que regula), retirar este
    // registro puede dejar huérfanos. Se pregunta antes (2_spec.md de la v2, RF9).
    private readonly IRepositorioReferencias _referencias;

    public ServicioAspectoNormativo(IRepositorioAspectoNormativo repositorio, IRepositorioReferencias referencias)
    {
        _repositorio = repositorio;
        _referencias = referencias;
    }

    public async Task<IEnumerable<AspectoNormativo>> ObtenerTodos(int limite)
    {
        // Regla de negocio: el límite debe ser positivo. La FORMA del dato es
        // correcta (sí es un entero), así que esto es 400 y no 422.
        if (limite <= 0)
        {
            throw new ArgumentException("El parámetro limite debe ser un número mayor a 0.");
        }

        return await _repositorio.ObtenerTodos(limite);
    }

    public async Task<AspectoNormativo> ObtenerPorId(int id)
    {
        var entidad = await _repositorio.ObtenerPorId(id);
        if (entidad == null)
        {
            throw new NoEncontradoExcepcion($"No existe un registro de aspecto_normativo con id {id}.");
        }

        return entidad;
    }

    public async Task Crear(AspectoNormativo entidad)
    {
        await _repositorio.Crear(entidad);
    }

    public async Task<int> Reemplazar(int id, AspectoNormativo entidad)
    {
        // La llave identifica la fila y viene de la ruta, no del cuerpo
        entidad.Id = id;

        var filasAfectadas = await _repositorio.Reemplazar(entidad);
        if (filasAfectadas == 0)
        {
            throw new NoEncontradoExcepcion($"No existe un registro de aspecto_normativo con id {id}.");
        }

        return filasAfectadas;
    }

    public async Task<int> ActualizarParcial(int id, string? tipo, string? descripcion, string? fuente)
    {
        // Sin esta comprobación el repositorio devolvería 0 filas, que en toda
        // la demás lógica significa "no existe" — y responderíamos 404 en vez
        // del 400 que exige el contrato para un cuerpo vacío.
        if (tipo == null && descripcion == null && fuente == null)
        {
            throw new ArgumentException("No se envió ningún campo para actualizar.");
        }

        var filasAfectadas = await _repositorio.ActualizarParcial(
            id, tipo, descripcion, fuente);

        if (filasAfectadas == 0)
        {
            throw new NoEncontradoExcepcion($"No existe un registro de aspecto_normativo con id {id}.");
        }

        return filasAfectadas;
    }

    public async Task<int> Eliminar(int id)
    {
        var dependientes = await _referencias.DependientesDeAspectoNormativo(id);
        if (dependientes.Count > 0)
        {
            throw new DependenciasActivasExcepcion(
                $"No se puede retirar el aspecto normativo {id}: tiene registros activos en {string.Join(", ", dependientes)}. Retírelos primero.");
        }

        var filasAfectadas = await _repositorio.EliminarLogico(id);
        if (filasAfectadas == 0)
        {
            throw new NoEncontradoExcepcion($"No existe un registro de aspecto_normativo con id {id}.");
        }

        return filasAfectadas;
    }
}
