using ApiInnovacion.Excepciones;
using ApiInnovacion.Modelos;
using ApiInnovacion.Repositorios;

namespace ApiInnovacion.Servicios;

/// <summary>
/// La capa 2: las reglas de negocio de activ_academica. Depende solo de interfaces
/// —su repositorio y el de referencias—, así que no sabe qué motor hay detrás y
/// se puede probar sin base de datos.
/// </summary>
public class ServicioActivAcademica : IServicioActivAcademica
{
    private readonly IRepositorioActivAcademica _repositorio;
    private readonly IRepositorioReferencias _referencias;

    public ServicioActivAcademica(IRepositorioActivAcademica repositorio, IRepositorioReferencias referencias)
    {
        _repositorio = repositorio;
        _referencias = referencias;
    }

    public async Task<IEnumerable<ActivAcademica>> ObtenerTodos(int limite)
    {
        // La FORMA del dato es correcta (sí es un entero): esto es 400, no 422.
        if (limite <= 0)
        {
            throw new ArgumentException("El parámetro limite debe ser un número mayor a 0.");
        }

        return await _repositorio.ObtenerTodos(limite);
    }

    public async Task<ActivAcademica> ObtenerPorId(int id)
    {
        var entidad = await _repositorio.ObtenerPorId(id);
        if (entidad == null)
        {
            throw new NoEncontradoExcepcion($"No existe un registro de activ_academica con id {id}.");
        }

        return entidad;
    }

    public async Task Crear(ActivAcademica entidad)
    {
        await ValidarReglas(entidad);
        await _repositorio.Crear(entidad);
    }

    public async Task<int> Reemplazar(int id, ActivAcademica entidad)
    {
        // La llave identifica la fila y viene de la ruta, no del cuerpo.
        entidad.Id = id;

        await ValidarReglas(entidad);

        var filasAfectadas = await _repositorio.Reemplazar(entidad);
        if (filasAfectadas == 0)
        {
            throw new NoEncontradoExcepcion($"No existe un registro de activ_academica con id {id}.");
        }

        return filasAfectadas;
    }

    public async Task<int> ActualizarParcial(int id, ActivAcademicaCambios cambios)
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
        if (cambios.NumCreditos != null) actual.NumCreditos = cambios.NumCreditos.Value;
        if (cambios.Tipo != null) actual.Tipo = cambios.Tipo;
        if (cambios.AreaFormacion != null) actual.AreaFormacion = cambios.AreaFormacion;
        if (cambios.HAcom != null) actual.HAcom = cambios.HAcom.Value;
        if (cambios.HIndep != null) actual.HIndep = cambios.HIndep.Value;
        if (cambios.Idioma != null) actual.Idioma = cambios.Idioma;
        if (cambios.Espejo != null) actual.Espejo = cambios.Espejo.Value;
        if (cambios.EntidadEspejo != null) actual.EntidadEspejo = cambios.EntidadEspejo;
        if (cambios.PaisEspejo != null) actual.PaisEspejo = cambios.PaisEspejo;
        if (cambios.Disenio != null) actual.Disenio = cambios.Disenio;
        await ValidarReglas(actual);

        var filasAfectadas = await _repositorio.ActualizarParcial(id, cambios);
        if (filasAfectadas == 0)
        {
            throw new NoEncontradoExcepcion($"No existe un registro de activ_academica con id {id}.");
        }

        return filasAfectadas;
    }

    public async Task<int> Eliminar(int id)
    {
        // Retirarlo con hijos activos los dejaría huérfanos: seguirían apuntando a
        // un registro que ya no aparece en ninguna lista.
        var dependientes = await _referencias.DependientesDeActivAcademica(id);
        if (dependientes.Count > 0)
        {
            throw new DependenciasActivasExcepcion(
                $"No se puede retirar la actividad académica {id}: tiene registros activos en {string.Join(", ", dependientes)}. Retírelos primero.");
        }

        var filasAfectadas = await _repositorio.EliminarLogico(id);
        if (filasAfectadas == 0)
        {
            throw new NoEncontradoExcepcion($"No existe un registro de activ_academica con id {id}.");
        }

        return filasAfectadas;
    }

    /// <summary>
    /// Las reglas que el motor NO puede defender solo: que cada clave foránea
    /// apunte a un registro ACTIVO (la FK del motor acepta uno retirado) y las
    /// reglas propias de activ_academica.
    /// </summary>
    private async Task ValidarReglas(ActivAcademica entidad)
    {
        // disenio es opcional: si viene vacío no hay nada que comprobar.
        if (entidad.Disenio != null && !await _referencias.ProgramaActivo(entidad.Disenio.Value))
        {
            throw new ReferenciaInvalidaExcepcion(
                $"El programa {entidad.Disenio} no existe o está retirado: el campo disenio debe apuntar a un registro activo.");
        }

        if (entidad.NumCreditos < 0)
        {
            throw new ArgumentException("El campo numCreditos no puede ser negativo.");
        }

        if (entidad.HAcom < 0)
        {
            throw new ArgumentException("El campo hAcom no puede ser negativo.");
        }

        if (entidad.HIndep < 0)
        {
            throw new ArgumentException("El campo hIndep no puede ser negativo.");
        }

        // espejo es una bandera guardada como TINYINT: solo 0 (no) o 1 (sí).
        if (entidad.Espejo > 1)
        {
            throw new ArgumentException("El campo espejo solo admite 0 (no) o 1 (sí).");
        }
    }
}
