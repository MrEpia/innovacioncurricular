using ApiInnovacion.Excepciones;
using ApiInnovacion.Modelos;
using ApiInnovacion.Repositorios;
using ApiInnovacion.Servicios;

namespace ApiInnovacion.Pruebas;

/// <summary>
/// Prueba de capas del bloque «Academico» de la v2: facultad, programa, acreditacion, registro_calificado.
/// Responsable: Sebastián Calvo. Solo su responsable edita este archivo.
///
/// Cada servicio corre contra su repositorio de mentiras y contra
/// RepositorioReferenciasFalso, donde cada prueba decide qué padres «existen»
/// y qué hijos «dependen»: así se comprueban las reglas de integridad sin SQL
/// Server.
/// </summary>
public static class PruebasAcademico
{
    public static async Task Ejecutar()
    {
        await ProbarFacultad();
        await ProbarPrograma();
        await ProbarAcreditacion();
        await ProbarRegistroCalificado();
    }

    private static async Task ProbarFacultad()
    {
        Console.WriteLine();
        Console.WriteLine("--- v2 · facultad ---");

        var referencias = new RepositorioReferenciasFalso();
        referencias.Universidades.Add(901);
        IServicioFacultad servicio = new ServicioFacultad(new RepositorioFacultadFalso(), referencias);

        Facultad Nuevo() => new Facultad
        {
            Id = 101,
            Nombre = "Nombre",
            Tipo = "Facultad",
            FechaFun = new DateTime(2024, 2, 1),
            Universidad = 901
        };
        var nuevo = Nuevo();

        await servicio.Crear(nuevo);
        Verificador.Revisar((await servicio.ObtenerTodos(1000)).Count() == 1,
            "facultad: se crea con sus padres activos y se lista.",
            "facultad: debía haber exactamente un registro.");

        await Verificador.EsperarExcepcion<NoEncontradoExcepcion>(
            () => servicio.ObtenerPorId(999999),
            "facultad: una llave inexistente lanza NoEncontradoExcepcion (404).");

        await Verificador.EsperarExcepcion<LlaveDuplicadaExcepcion>(
            () => servicio.Crear(Nuevo()),
            "facultad: la llave repetida lanza LlaveDuplicadaExcepcion (409).");

        await Verificador.EsperarExcepcion<ReferenciaInvalidaExcepcion>(
            () => { var x = Nuevo(); x.Universidad = 88888; return servicio.Crear(x); },
            "facultad: universidad hacia un padre inexistente o retirado lanza ReferenciaInvalidaExcepcion (400).");

        await Verificador.EsperarExcepcion<ArgumentException>(
            () => { var x = Nuevo(); x.Tipo = "Otro"; return servicio.Crear(x); },
            "facultad: un tipo fuera de la lista lanza ArgumentException (400).");

        await Verificador.EsperarExcepcion<ArgumentException>(
            () => servicio.ActualizarParcial(nuevo.Id, new FacultadCambios()),
            "facultad: un PATCH vacío lanza ArgumentException (400), no NoEncontrado.");

        await servicio.ActualizarParcial(nuevo.Id, new FacultadCambios { Nombre = "Cambiado" });
        Verificador.Revisar((await servicio.ObtenerPorId(nuevo.Id)).Nombre == "Cambiado",
            "facultad: el PATCH escribe solo lo enviado.",
            "facultad: el PATCH no escribió nombre.");

        referencias.Dependientes[$"facultad:{nuevo.Id}"] = new() { "hija (1)" };
        await Verificador.EsperarExcepcion<DependenciasActivasExcepcion>(
            () => servicio.Eliminar(nuevo.Id),
            "facultad: con hijos activos no se puede retirar (409).");
        referencias.Dependientes.Clear();

        await servicio.Eliminar(nuevo.Id);
        await Verificador.EsperarExcepcion<NoEncontradoExcepcion>(
            () => servicio.Eliminar(nuevo.Id),
            "facultad: el segundo retiro lanza NoEncontradoExcepcion (404).");
        Verificador.Revisar(!(await servicio.ObtenerTodos(1000)).Any(),
            "facultad: tras retirarlo, el listado vuelve a estar vacío.",
            "facultad: debía quedar vacío.");
    }

    private static async Task ProbarPrograma()
    {
        Console.WriteLine();
        Console.WriteLine("--- v2 · programa ---");

        var referencias = new RepositorioReferenciasFalso();
        referencias.Facultades.Add(901);
        IServicioPrograma servicio = new ServicioPrograma(new RepositorioProgramaFalso(), referencias);

        Programa Nuevo() => new Programa
        {
            Id = 101,
            Nombre = "Nombre",
            Tipo = "Tipo",
            Nivel = "Nivel",
            FechaCreacion = "Fecha de creación",
            FechaCierre = "Fecha de cierre",
            NumeroCohortes = "Número de cohortes",
            CantGraduados = "Cantidad de graduado",
            FechaActualizacion = "Fecha de actualizaci",
            Ciudad = "Ciudad",
            Facultad = 901
        };
        var nuevo = Nuevo();

        await servicio.Crear(nuevo);
        Verificador.Revisar((await servicio.ObtenerTodos(1000)).Count() == 1,
            "programa: se crea con sus padres activos y se lista.",
            "programa: debía haber exactamente un registro.");

        await Verificador.EsperarExcepcion<NoEncontradoExcepcion>(
            () => servicio.ObtenerPorId(999999),
            "programa: una llave inexistente lanza NoEncontradoExcepcion (404).");

        await Verificador.EsperarExcepcion<LlaveDuplicadaExcepcion>(
            () => servicio.Crear(Nuevo()),
            "programa: la llave repetida lanza LlaveDuplicadaExcepcion (409).");

        await Verificador.EsperarExcepcion<ReferenciaInvalidaExcepcion>(
            () => { var x = Nuevo(); x.Facultad = 88888; return servicio.Crear(x); },
            "programa: facultad hacia un padre inexistente o retirado lanza ReferenciaInvalidaExcepcion (400).");

        await Verificador.EsperarExcepcion<ArgumentException>(
            () => servicio.ActualizarParcial(nuevo.Id, new ProgramaCambios()),
            "programa: un PATCH vacío lanza ArgumentException (400), no NoEncontrado.");

        await servicio.ActualizarParcial(nuevo.Id, new ProgramaCambios { Nombre = "Cambiado" });
        Verificador.Revisar((await servicio.ObtenerPorId(nuevo.Id)).Nombre == "Cambiado",
            "programa: el PATCH escribe solo lo enviado.",
            "programa: el PATCH no escribió nombre.");

        referencias.Dependientes[$"programa:{nuevo.Id}"] = new() { "hija (1)" };
        await Verificador.EsperarExcepcion<DependenciasActivasExcepcion>(
            () => servicio.Eliminar(nuevo.Id),
            "programa: con hijos activos no se puede retirar (409).");
        referencias.Dependientes.Clear();

        await servicio.Eliminar(nuevo.Id);
        await Verificador.EsperarExcepcion<NoEncontradoExcepcion>(
            () => servicio.Eliminar(nuevo.Id),
            "programa: el segundo retiro lanza NoEncontradoExcepcion (404).");
        Verificador.Revisar(!(await servicio.ObtenerTodos(1000)).Any(),
            "programa: tras retirarlo, el listado vuelve a estar vacío.",
            "programa: debía quedar vacío.");
    }

    private static async Task ProbarAcreditacion()
    {
        Console.WriteLine();
        Console.WriteLine("--- v2 · acreditacion ---");

        var referencias = new RepositorioReferenciasFalso();
        referencias.Programas.Add(901);
        IServicioAcreditacion servicio = new ServicioAcreditacion(new RepositorioAcreditacionFalso(), referencias);

        Acreditacion Nuevo() => new Acreditacion
        {
            Resolucion = 101,
            Tipo = "Tipo",
            Calificacion = "Calificación",
            FechaInicio = "Fecha de inicio",
            FechaFin = "Fecha de fin",
            Programa = 901
        };
        var nuevo = Nuevo();

        await servicio.Crear(nuevo);
        Verificador.Revisar((await servicio.ObtenerTodos(1000)).Count() == 1,
            "acreditacion: se crea con sus padres activos y se lista.",
            "acreditacion: debía haber exactamente un registro.");

        await Verificador.EsperarExcepcion<NoEncontradoExcepcion>(
            () => servicio.ObtenerPorResolucion(999999),
            "acreditacion: una llave inexistente lanza NoEncontradoExcepcion (404).");

        await Verificador.EsperarExcepcion<LlaveDuplicadaExcepcion>(
            () => servicio.Crear(Nuevo()),
            "acreditacion: la llave repetida lanza LlaveDuplicadaExcepcion (409).");

        await Verificador.EsperarExcepcion<ReferenciaInvalidaExcepcion>(
            () => { var x = Nuevo(); x.Programa = 88888; return servicio.Crear(x); },
            "acreditacion: programa hacia un padre inexistente o retirado lanza ReferenciaInvalidaExcepcion (400).");

        await Verificador.EsperarExcepcion<ArgumentException>(
            () => servicio.ActualizarParcial(nuevo.Resolucion, new AcreditacionCambios()),
            "acreditacion: un PATCH vacío lanza ArgumentException (400), no NoEncontrado.");

        await servicio.ActualizarParcial(nuevo.Resolucion, new AcreditacionCambios { Calificacion = "Cambiado" });
        Verificador.Revisar((await servicio.ObtenerPorResolucion(nuevo.Resolucion)).Calificacion == "Cambiado",
            "acreditacion: el PATCH escribe solo lo enviado.",
            "acreditacion: el PATCH no escribió calificacion.");

        await servicio.Eliminar(nuevo.Resolucion);
        await Verificador.EsperarExcepcion<NoEncontradoExcepcion>(
            () => servicio.Eliminar(nuevo.Resolucion),
            "acreditacion: el segundo retiro lanza NoEncontradoExcepcion (404).");
        Verificador.Revisar(!(await servicio.ObtenerTodos(1000)).Any(),
            "acreditacion: tras retirarlo, el listado vuelve a estar vacío.",
            "acreditacion: debía quedar vacío.");
    }

    private static async Task ProbarRegistroCalificado()
    {
        Console.WriteLine();
        Console.WriteLine("--- v2 · registro_calificado ---");

        var referencias = new RepositorioReferenciasFalso();
        referencias.Programas.Add(901);
        IServicioRegistroCalificado servicio = new ServicioRegistroCalificado(new RepositorioRegistroCalificadoFalso(), referencias);

        RegistroCalificado Nuevo() => new RegistroCalificado
        {
            Codigo = 101,
            CantCreditos = "Créditos",
            HoraAcom = "Horas de acompañamie",
            HoraInd = "Horas independientes",
            Metodologia = "Metodología",
            FechaInicio = new DateTime(2024, 2, 1),
            FechaFin = new DateTime(2024, 2, 1),
            DuracionAnios = "Duración (años)",
            DuracionSemestres = "Duración (semestres)",
            TipoTitulacion = "Tipo de titulación",
            Programa = 901
        };
        var nuevo = Nuevo();

        await servicio.Crear(nuevo);
        Verificador.Revisar((await servicio.ObtenerTodos(1000)).Count() == 1,
            "registro_calificado: se crea con sus padres activos y se lista.",
            "registro_calificado: debía haber exactamente un registro.");

        await Verificador.EsperarExcepcion<NoEncontradoExcepcion>(
            () => servicio.ObtenerPorCodigo(999999),
            "registro_calificado: una llave inexistente lanza NoEncontradoExcepcion (404).");

        await Verificador.EsperarExcepcion<LlaveDuplicadaExcepcion>(
            () => servicio.Crear(Nuevo()),
            "registro_calificado: la llave repetida lanza LlaveDuplicadaExcepcion (409).");

        await Verificador.EsperarExcepcion<ReferenciaInvalidaExcepcion>(
            () => { var x = Nuevo(); x.Programa = 88888; return servicio.Crear(x); },
            "registro_calificado: programa hacia un padre inexistente o retirado lanza ReferenciaInvalidaExcepcion (400).");

        await Verificador.EsperarExcepcion<ArgumentException>(
            () => { var x = Nuevo(); x.FechaInicio = new DateTime(2024, 3, 1); x.FechaFin = new DateTime(2024, 1, 1); return servicio.Crear(x); },
            "registro_calificado: fechaFin anterior a fechaInicio lanza ArgumentException (400).");

        await Verificador.EsperarExcepcion<ArgumentException>(
            () => servicio.ActualizarParcial(nuevo.Codigo, new RegistroCalificadoCambios()),
            "registro_calificado: un PATCH vacío lanza ArgumentException (400), no NoEncontrado.");

        await servicio.ActualizarParcial(nuevo.Codigo, new RegistroCalificadoCambios { CantCreditos = "Cambiado" });
        Verificador.Revisar((await servicio.ObtenerPorCodigo(nuevo.Codigo)).CantCreditos == "Cambiado",
            "registro_calificado: el PATCH escribe solo lo enviado.",
            "registro_calificado: el PATCH no escribió cant_creditos.");

        await Verificador.EsperarExcepcion<ArgumentException>(
            () => servicio.ActualizarParcial(nuevo.Codigo, new RegistroCalificadoCambios { FechaFin = new DateTime(2000, 1, 1) }),
            "registro_calificado: un PATCH que solo trae fechaFin se valida contra el fechaInicio guardado.");

        referencias.Dependientes[$"registro_calificado:{nuevo.Codigo}"] = new() { "hija (1)" };
        await Verificador.EsperarExcepcion<DependenciasActivasExcepcion>(
            () => servicio.Eliminar(nuevo.Codigo),
            "registro_calificado: con hijos activos no se puede retirar (409).");
        referencias.Dependientes.Clear();

        await servicio.Eliminar(nuevo.Codigo);
        await Verificador.EsperarExcepcion<NoEncontradoExcepcion>(
            () => servicio.Eliminar(nuevo.Codigo),
            "registro_calificado: el segundo retiro lanza NoEncontradoExcepcion (404).");
        Verificador.Revisar(!(await servicio.ObtenerTodos(1000)).Any(),
            "registro_calificado: tras retirarlo, el listado vuelve a estar vacío.",
            "registro_calificado: debía quedar vacío.");
    }
}

/// <summary>El repositorio de mentiras de facultad: un diccionario en memoria.</summary>
public class RepositorioFacultadFalso : IRepositorioFacultad
{
    private readonly Dictionary<string, Facultad> _filas = new();

    public Task<IEnumerable<Facultad>> ObtenerTodos(int limite) =>
        Task.FromResult(_filas.Values.Take(limite).AsEnumerable());

    public Task<Facultad?> ObtenerPorId(int id) =>
        Task.FromResult(_filas.TryGetValue($"{id}", out var f) ? f : null);

    public Task Crear(Facultad entidad)
    {
        var clave = $"{entidad.Id}";
        // Lo mismo que hace la PK del motor, traducido como lo traduce el
        // repositorio real.
        if (_filas.ContainsKey(clave)) throw new LlaveDuplicadaExcepcion("llave repetida");
        _filas[clave] = entidad;
        return Task.CompletedTask;
    }

    public Task<int> Reemplazar(Facultad entidad)
    {
        var clave = $"{entidad.Id}";
        if (!_filas.ContainsKey(clave)) return Task.FromResult(0);
        _filas[clave] = entidad;
        return Task.FromResult(1);
    }

    public Task<int> ActualizarParcial(int id, FacultadCambios cambios)
    {
        if (!_filas.TryGetValue($"{id}", out var f)) return Task.FromResult(0);
        if (cambios.Nombre != null) f.Nombre = cambios.Nombre;
        if (cambios.Tipo != null) f.Tipo = cambios.Tipo;
        if (cambios.FechaFun != null) f.FechaFun = cambios.FechaFun.Value;
        if (cambios.Universidad != null) f.Universidad = cambios.Universidad.Value;
        return Task.FromResult(1);
    }

    public Task<int> EliminarLogico(int id) =>
        Task.FromResult(_filas.Remove($"{id}") ? 1 : 0);
}

/// <summary>El repositorio de mentiras de programa: un diccionario en memoria.</summary>
public class RepositorioProgramaFalso : IRepositorioPrograma
{
    private readonly Dictionary<string, Programa> _filas = new();

    public Task<IEnumerable<Programa>> ObtenerTodos(int limite) =>
        Task.FromResult(_filas.Values.Take(limite).AsEnumerable());

    public Task<Programa?> ObtenerPorId(int id) =>
        Task.FromResult(_filas.TryGetValue($"{id}", out var f) ? f : null);

    public Task Crear(Programa entidad)
    {
        var clave = $"{entidad.Id}";
        // Lo mismo que hace la PK del motor, traducido como lo traduce el
        // repositorio real.
        if (_filas.ContainsKey(clave)) throw new LlaveDuplicadaExcepcion("llave repetida");
        _filas[clave] = entidad;
        return Task.CompletedTask;
    }

    public Task<int> Reemplazar(Programa entidad)
    {
        var clave = $"{entidad.Id}";
        if (!_filas.ContainsKey(clave)) return Task.FromResult(0);
        _filas[clave] = entidad;
        return Task.FromResult(1);
    }

    public Task<int> ActualizarParcial(int id, ProgramaCambios cambios)
    {
        if (!_filas.TryGetValue($"{id}", out var f)) return Task.FromResult(0);
        if (cambios.Nombre != null) f.Nombre = cambios.Nombre;
        if (cambios.Tipo != null) f.Tipo = cambios.Tipo;
        if (cambios.Nivel != null) f.Nivel = cambios.Nivel;
        if (cambios.FechaCreacion != null) f.FechaCreacion = cambios.FechaCreacion;
        if (cambios.FechaCierre != null) f.FechaCierre = cambios.FechaCierre;
        if (cambios.NumeroCohortes != null) f.NumeroCohortes = cambios.NumeroCohortes;
        if (cambios.CantGraduados != null) f.CantGraduados = cambios.CantGraduados;
        if (cambios.FechaActualizacion != null) f.FechaActualizacion = cambios.FechaActualizacion;
        if (cambios.Ciudad != null) f.Ciudad = cambios.Ciudad;
        if (cambios.Facultad != null) f.Facultad = cambios.Facultad.Value;
        return Task.FromResult(1);
    }

    public Task<int> EliminarLogico(int id) =>
        Task.FromResult(_filas.Remove($"{id}") ? 1 : 0);
}

/// <summary>El repositorio de mentiras de acreditacion: un diccionario en memoria.</summary>
public class RepositorioAcreditacionFalso : IRepositorioAcreditacion
{
    private readonly Dictionary<string, Acreditacion> _filas = new();

    public Task<IEnumerable<Acreditacion>> ObtenerTodos(int limite) =>
        Task.FromResult(_filas.Values.Take(limite).AsEnumerable());

    public Task<Acreditacion?> ObtenerPorResolucion(int resolucion) =>
        Task.FromResult(_filas.TryGetValue($"{resolucion}", out var f) ? f : null);

    public Task Crear(Acreditacion entidad)
    {
        var clave = $"{entidad.Resolucion}";
        // Lo mismo que hace la PK del motor, traducido como lo traduce el
        // repositorio real.
        if (_filas.ContainsKey(clave)) throw new LlaveDuplicadaExcepcion("llave repetida");
        _filas[clave] = entidad;
        return Task.CompletedTask;
    }

    public Task<int> Reemplazar(Acreditacion entidad)
    {
        var clave = $"{entidad.Resolucion}";
        if (!_filas.ContainsKey(clave)) return Task.FromResult(0);
        _filas[clave] = entidad;
        return Task.FromResult(1);
    }

    public Task<int> ActualizarParcial(int resolucion, AcreditacionCambios cambios)
    {
        if (!_filas.TryGetValue($"{resolucion}", out var f)) return Task.FromResult(0);
        if (cambios.Tipo != null) f.Tipo = cambios.Tipo;
        if (cambios.Calificacion != null) f.Calificacion = cambios.Calificacion;
        if (cambios.FechaInicio != null) f.FechaInicio = cambios.FechaInicio;
        if (cambios.FechaFin != null) f.FechaFin = cambios.FechaFin;
        if (cambios.Programa != null) f.Programa = cambios.Programa.Value;
        return Task.FromResult(1);
    }

    public Task<int> EliminarLogico(int resolucion) =>
        Task.FromResult(_filas.Remove($"{resolucion}") ? 1 : 0);
}

/// <summary>El repositorio de mentiras de registro_calificado: un diccionario en memoria.</summary>
public class RepositorioRegistroCalificadoFalso : IRepositorioRegistroCalificado
{
    private readonly Dictionary<string, RegistroCalificado> _filas = new();

    public Task<IEnumerable<RegistroCalificado>> ObtenerTodos(int limite) =>
        Task.FromResult(_filas.Values.Take(limite).AsEnumerable());

    public Task<RegistroCalificado?> ObtenerPorCodigo(int codigo) =>
        Task.FromResult(_filas.TryGetValue($"{codigo}", out var f) ? f : null);

    public Task Crear(RegistroCalificado entidad)
    {
        var clave = $"{entidad.Codigo}";
        // Lo mismo que hace la PK del motor, traducido como lo traduce el
        // repositorio real.
        if (_filas.ContainsKey(clave)) throw new LlaveDuplicadaExcepcion("llave repetida");
        _filas[clave] = entidad;
        return Task.CompletedTask;
    }

    public Task<int> Reemplazar(RegistroCalificado entidad)
    {
        var clave = $"{entidad.Codigo}";
        if (!_filas.ContainsKey(clave)) return Task.FromResult(0);
        _filas[clave] = entidad;
        return Task.FromResult(1);
    }

    public Task<int> ActualizarParcial(int codigo, RegistroCalificadoCambios cambios)
    {
        if (!_filas.TryGetValue($"{codigo}", out var f)) return Task.FromResult(0);
        if (cambios.CantCreditos != null) f.CantCreditos = cambios.CantCreditos;
        if (cambios.HoraAcom != null) f.HoraAcom = cambios.HoraAcom;
        if (cambios.HoraInd != null) f.HoraInd = cambios.HoraInd;
        if (cambios.Metodologia != null) f.Metodologia = cambios.Metodologia;
        if (cambios.FechaInicio != null) f.FechaInicio = cambios.FechaInicio.Value;
        if (cambios.FechaFin != null) f.FechaFin = cambios.FechaFin.Value;
        if (cambios.DuracionAnios != null) f.DuracionAnios = cambios.DuracionAnios;
        if (cambios.DuracionSemestres != null) f.DuracionSemestres = cambios.DuracionSemestres;
        if (cambios.TipoTitulacion != null) f.TipoTitulacion = cambios.TipoTitulacion;
        if (cambios.Programa != null) f.Programa = cambios.Programa.Value;
        return Task.FromResult(1);
    }

    public Task<int> EliminarLogico(int codigo) =>
        Task.FromResult(_filas.Remove($"{codigo}") ? 1 : 0);
}
