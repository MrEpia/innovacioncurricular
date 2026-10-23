using ApiInnovacion.Excepciones;
using ApiInnovacion.Modelos;
using ApiInnovacion.Repositorios;

namespace ApiInnovacion.Servicios;

/// <summary>
/// La capa 2: las reglas de negocio de docente_departamento. Depende solo de interfaces
/// —su repositorio y el de referencias—, así que no sabe qué motor hay detrás y
/// se puede probar sin base de datos.
/// </summary>
public class ServicioDocenteDepartamento : IServicioDocenteDepartamento
{
    private readonly IRepositorioDocenteDepartamento _repositorio;
    private readonly IRepositorioReferencias _referencias;

    private static readonly string[] DedicacionesValidas = { "Tiempo Completo", "Medio Tiempo", "Cátedra" };

    public ServicioDocenteDepartamento(IRepositorioDocenteDepartamento repositorio, IRepositorioReferencias referencias)
    {
        _repositorio = repositorio;
        _referencias = referencias;
    }

    public async Task<IEnumerable<DocenteDepartamento>> ObtenerTodos(int limite)
    {
        // La FORMA del dato es correcta (sí es un entero): esto es 400, no 422.
        if (limite <= 0)
        {
            throw new ArgumentException("El parámetro limite debe ser un número mayor a 0.");
        }

        return await _repositorio.ObtenerTodos(limite);
    }

    public async Task<DocenteDepartamento> ObtenerPorLlave(int docente, int departamento)
    {
        var entidad = await _repositorio.ObtenerPorLlave(docente, departamento);
        if (entidad == null)
        {
            throw new NoEncontradoExcepcion($"No existe un registro de docente_departamento con docente {docente}, departamento {departamento}.");
        }

        return entidad;
    }

    public async Task Crear(DocenteDepartamento entidad)
    {
        await ValidarReglas(entidad);
        await _repositorio.Crear(entidad);
    }

    public async Task<int> Reemplazar(int docente, int departamento, DocenteDepartamento entidad)
    {
        // La llave identifica la fila y viene de la ruta, no del cuerpo.
        entidad.Docente = docente;
        entidad.Departamento = departamento;

        await ValidarReglas(entidad);

        var filasAfectadas = await _repositorio.Reemplazar(entidad);
        if (filasAfectadas == 0)
        {
            throw new NoEncontradoExcepcion($"No existe un registro de docente_departamento con docente {docente}, departamento {departamento}.");
        }

        return filasAfectadas;
    }

    public async Task<int> ActualizarParcial(int docente, int departamento, DocenteDepartamentoCambios cambios)
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
        var actual = await ObtenerPorLlave(docente, departamento);
        if (cambios.Dedicacion != null) actual.Dedicacion = cambios.Dedicacion;
        if (cambios.Modalidad != null) actual.Modalidad = cambios.Modalidad;
        if (cambios.FechaIngreso != null) actual.FechaIngreso = cambios.FechaIngreso.Value;
        if (cambios.FechaSalida != null) actual.FechaSalida = cambios.FechaSalida;
        await ValidarReglas(actual);

        var filasAfectadas = await _repositorio.ActualizarParcial(docente, departamento, cambios);
        if (filasAfectadas == 0)
        {
            throw new NoEncontradoExcepcion($"No existe un registro de docente_departamento con docente {docente}, departamento {departamento}.");
        }

        return filasAfectadas;
    }

    public async Task<int> Eliminar(int docente, int departamento)
    {
        var filasAfectadas = await _repositorio.EliminarLogico(docente, departamento);
        if (filasAfectadas == 0)
        {
            throw new NoEncontradoExcepcion($"No existe un registro de docente_departamento con docente {docente}, departamento {departamento}.");
        }

        return filasAfectadas;
    }

    /// <summary>
    /// Las reglas que el motor NO puede defender solo: que cada clave foránea
    /// apunte a un registro ACTIVO (la FK del motor acepta uno retirado) y las
    /// reglas propias de docente_departamento.
    /// </summary>
    private async Task ValidarReglas(DocenteDepartamento entidad)
    {
        if (!await _referencias.ProgramaActivo(entidad.Departamento))
        {
            throw new ReferenciaInvalidaExcepcion(
                $"El programa {entidad.Departamento} no existe o está retirado: el campo departamento debe apuntar a un registro activo.");
        }

        // Las tres dedicaciones que define el módulo para un docente.
        if (!DedicacionesValidas.Contains(entidad.Dedicacion))
        {
            throw new ArgumentException(
                $"La dedicación debe ser una de: {string.Join(", ", DedicacionesValidas)}.");
        }

        // Una fecha de cierre anterior a la de apertura es un dato imposible: la
        // base lo aceptaría, el negocio no.
        if (entidad.FechaSalida != null && entidad.FechaSalida < entidad.FechaIngreso)
        {
            throw new ArgumentException("El campo fechaSalida no puede ser anterior a fechaIngreso.");
        }
    }
}
