using ApiInnovacion.Excepciones;
using ApiInnovacion.Modelos;
using ApiInnovacion.Repositorios;

namespace ApiInnovacion.Servicios;

/// <summary>
/// La capa 2: las reglas de negocio de premio. Depende solo de interfaces
/// —su repositorio y el de referencias—, así que no sabe qué motor hay detrás y
/// se puede probar sin base de datos.
/// </summary>
public class ServicioPremio : IServicioPremio
{
    private readonly IRepositorioPremio _repositorio;
    private readonly IRepositorioReferencias _referencias;

    public ServicioPremio(IRepositorioPremio repositorio, IRepositorioReferencias referencias)
    {
        _repositorio = repositorio;
        _referencias = referencias;
    }

    public async Task<IEnumerable<Premio>> ObtenerTodos(int limite)
    {
        // La FORMA del dato es correcta (sí es un entero): esto es 400, no 422.
        if (limite <= 0)
        {
            throw new ArgumentException("El parámetro limite debe ser un número mayor a 0.");
        }

        return await _repositorio.ObtenerTodos(limite);
    }

    public async Task<Premio> ObtenerPorId(int id)
    {
        var entidad = await _repositorio.ObtenerPorId(id);
        if (entidad == null)
        {
            throw new NoEncontradoExcepcion($"No existe un registro de premio con id {id}.");
        }

        return entidad;
    }

    public async Task Crear(Premio entidad)
    {
        await ValidarReglas(entidad);
        await _repositorio.Crear(entidad);
    }

    public async Task<int> Reemplazar(int id, Premio entidad)
    {
        // La llave identifica la fila y viene de la ruta, no del cuerpo.
        entidad.Id = id;

        await ValidarReglas(entidad);

        var filasAfectadas = await _repositorio.Reemplazar(entidad);
        if (filasAfectadas == 0)
        {
            throw new NoEncontradoExcepcion($"No existe un registro de premio con id {id}.");
        }

        return filasAfectadas;
    }

    public async Task<int> ActualizarParcial(int id, PremioCambios cambios)
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
        if (cambios.Descripcion != null) actual.Descripcion = cambios.Descripcion;
        if (cambios.Fecha != null) actual.Fecha = cambios.Fecha.Value;
        if (cambios.EntidadOtorgante != null) actual.EntidadOtorgante = cambios.EntidadOtorgante;
        if (cambios.Pais != null) actual.Pais = cambios.Pais;
        if (cambios.Programa != null) actual.Programa = cambios.Programa.Value;
        await ValidarReglas(actual);

        var filasAfectadas = await _repositorio.ActualizarParcial(id, cambios);
        if (filasAfectadas == 0)
        {
            throw new NoEncontradoExcepcion($"No existe un registro de premio con id {id}.");
        }

        return filasAfectadas;
    }

    public async Task<int> Eliminar(int id)
    {
        var filasAfectadas = await _repositorio.EliminarLogico(id);
        if (filasAfectadas == 0)
        {
            throw new NoEncontradoExcepcion($"No existe un registro de premio con id {id}.");
        }

        return filasAfectadas;
    }

    /// <summary>
    /// Las reglas que el motor NO puede defender solo: que cada clave foránea
    /// apunte a un registro ACTIVO (la FK del motor acepta uno retirado) y las
    /// reglas propias de premio.
    /// </summary>
    private async Task ValidarReglas(Premio entidad)
    {
        if (!await _referencias.ProgramaActivo(entidad.Programa))
        {
            throw new ReferenciaInvalidaExcepcion(
                $"El programa {entidad.Programa} no existe o está retirado: el campo programa debe apuntar a un registro activo.");
        }
    }
}
