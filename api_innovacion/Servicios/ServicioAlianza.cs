using ApiInnovacion.Excepciones;
using ApiInnovacion.Modelos;
using ApiInnovacion.Repositorios;

namespace ApiInnovacion.Servicios;

/// <summary>
/// La capa 2: las reglas de negocio de alianza. Depende solo de interfaces
/// —su repositorio y el de referencias—, así que no sabe qué motor hay detrás y
/// se puede probar sin base de datos.
/// </summary>
public class ServicioAlianza : IServicioAlianza
{
    private readonly IRepositorioAlianza _repositorio;
    private readonly IRepositorioReferencias _referencias;

    public ServicioAlianza(IRepositorioAlianza repositorio, IRepositorioReferencias referencias)
    {
        _repositorio = repositorio;
        _referencias = referencias;
    }

    public async Task<IEnumerable<Alianza>> ObtenerTodos(int limite)
    {
        // La FORMA del dato es correcta (sí es un entero): esto es 400, no 422.
        if (limite <= 0)
        {
            throw new ArgumentException("El parámetro limite debe ser un número mayor a 0.");
        }

        return await _repositorio.ObtenerTodos(limite);
    }

    public async Task<Alianza> ObtenerPorLlave(int aliado, int departamento)
    {
        var entidad = await _repositorio.ObtenerPorLlave(aliado, departamento);
        if (entidad == null)
        {
            throw new NoEncontradoExcepcion($"No existe un registro de alianza con aliado {aliado}, departamento {departamento}.");
        }

        return entidad;
    }

    public async Task Crear(Alianza entidad)
    {
        await ValidarReglas(entidad);
        await _repositorio.Crear(entidad);
    }

    public async Task<int> Reemplazar(int aliado, int departamento, Alianza entidad)
    {
        // La llave identifica la fila y viene de la ruta, no del cuerpo.
        entidad.Aliado = aliado;
        entidad.Departamento = departamento;

        await ValidarReglas(entidad);

        var filasAfectadas = await _repositorio.Reemplazar(entidad);
        if (filasAfectadas == 0)
        {
            throw new NoEncontradoExcepcion($"No existe un registro de alianza con aliado {aliado}, departamento {departamento}.");
        }

        return filasAfectadas;
    }

    public async Task<int> ActualizarParcial(int aliado, int departamento, AlianzaCambios cambios)
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
        var actual = await ObtenerPorLlave(aliado, departamento);
        if (cambios.FechaInicio != null) actual.FechaInicio = cambios.FechaInicio.Value;
        if (cambios.FechaFin != null) actual.FechaFin = cambios.FechaFin;
        if (cambios.Docente != null) actual.Docente = cambios.Docente;
        await ValidarReglas(actual);

        var filasAfectadas = await _repositorio.ActualizarParcial(aliado, departamento, cambios);
        if (filasAfectadas == 0)
        {
            throw new NoEncontradoExcepcion($"No existe un registro de alianza con aliado {aliado}, departamento {departamento}.");
        }

        return filasAfectadas;
    }

    public async Task<int> Eliminar(int aliado, int departamento)
    {
        var filasAfectadas = await _repositorio.EliminarLogico(aliado, departamento);
        if (filasAfectadas == 0)
        {
            throw new NoEncontradoExcepcion($"No existe un registro de alianza con aliado {aliado}, departamento {departamento}.");
        }

        return filasAfectadas;
    }

    /// <summary>
    /// Las reglas que el motor NO puede defender solo: que cada clave foránea
    /// apunte a un registro ACTIVO (la FK del motor acepta uno retirado) y las
    /// reglas propias de alianza.
    /// </summary>
    private async Task ValidarReglas(Alianza entidad)
    {
        if (!await _referencias.AliadoActivo(entidad.Aliado))
        {
            throw new ReferenciaInvalidaExcepcion(
                $"El aliado {entidad.Aliado} no existe o está retirado: el campo aliado debe apuntar a un registro activo.");
        }

        if (!await _referencias.ProgramaActivo(entidad.Departamento))
        {
            throw new ReferenciaInvalidaExcepcion(
                $"El programa {entidad.Departamento} no existe o está retirado: el campo departamento debe apuntar a un registro activo.");
        }

        // Una fecha de cierre anterior a la de apertura es un dato imposible: la
        // base lo aceptaría, el negocio no.
        if (entidad.FechaFin != null && entidad.FechaFin < entidad.FechaInicio)
        {
            throw new ArgumentException("El campo fechaFin no puede ser anterior a fechaInicio.");
        }
    }
}
