using ApiInnovacion.Excepciones;
using ApiInnovacion.Modelos;
using ApiInnovacion.Repositorios;
using ApiInnovacion.Servicios;

namespace ApiInnovacion.Pruebas;

/// <summary>
/// Prueba de capas del bloque «Puentes» de la v2: programa_ac, programa_pe, programa_ci, an_programa, enfoque_rc.
/// Responsable: David Epia. Solo su responsable edita este archivo.
///
/// Cada servicio corre contra su repositorio de mentiras y contra
/// RepositorioReferenciasFalso, donde cada prueba decide qué padres «existen»
/// y qué hijos «dependen»: así se comprueban las reglas de integridad sin SQL
/// Server.
/// </summary>
public static class PruebasPuentes
{
    public static async Task Ejecutar()
    {
        await ProbarProgramaAc();
        await ProbarProgramaPe();
        await ProbarProgramaCi();
        await ProbarAnPrograma();
        await ProbarEnfoqueRc();
    }

    private static async Task ProbarProgramaAc()
    {
        Console.WriteLine();
        Console.WriteLine("--- v2 · programa_ac ---");

        var referencias = new RepositorioReferenciasFalso();
        referencias.Programas.Add(901);
        referencias.AreasConocimiento.Add("1A01");
        IServicioProgramaAc servicio = new ServicioProgramaAc(new RepositorioProgramaAcFalso(), referencias);

        ProgramaAc Nuevo() => new ProgramaAc
        {
            Programa = 901,
            AreaConocimiento = "1A01"
        };
        var nuevo = Nuevo();

        await servicio.Crear(nuevo);
        Verificador.Revisar((await servicio.ObtenerTodos(1000)).Count() == 1,
            "programa_ac: se crea con sus padres activos y se lista.",
            "programa_ac: debía haber exactamente un registro.");

        await Verificador.EsperarExcepcion<NoEncontradoExcepcion>(
            () => servicio.ObtenerPorLlave(999999, "NOEXISTE"),
            "programa_ac: una llave inexistente lanza NoEncontradoExcepcion (404).");

        await Verificador.EsperarExcepcion<LlaveDuplicadaExcepcion>(
            () => servicio.Crear(Nuevo()),
            "programa_ac: la llave repetida lanza LlaveDuplicadaExcepcion (409).");

        await Verificador.EsperarExcepcion<ReferenciaInvalidaExcepcion>(
            () => { var x = Nuevo(); x.Programa = 88888; return servicio.Crear(x); },
            "programa_ac: programa hacia un padre inexistente o retirado lanza ReferenciaInvalidaExcepcion (400).");

        await Verificador.EsperarExcepcion<ReferenciaInvalidaExcepcion>(
            () => { var x = Nuevo(); x.AreaConocimiento = "ZZ99"; return servicio.Crear(x); },
            "programa_ac: area_conocimiento hacia un padre inexistente o retirado lanza ReferenciaInvalidaExcepcion (400).");

        await servicio.Eliminar(nuevo.Programa, nuevo.AreaConocimiento);
        await Verificador.EsperarExcepcion<NoEncontradoExcepcion>(
            () => servicio.Eliminar(nuevo.Programa, nuevo.AreaConocimiento),
            "programa_ac: el segundo retiro lanza NoEncontradoExcepcion (404).");
        Verificador.Revisar(!(await servicio.ObtenerTodos(1000)).Any(),
            "programa_ac: tras retirarlo, el listado vuelve a estar vacío.",
            "programa_ac: debía quedar vacío.");
    }

    private static async Task ProbarProgramaPe()
    {
        Console.WriteLine();
        Console.WriteLine("--- v2 · programa_pe ---");

        var referencias = new RepositorioReferenciasFalso();
        referencias.Programas.Add(901);
        referencias.PracticasEstrategias.Add(901);
        IServicioProgramaPe servicio = new ServicioProgramaPe(new RepositorioProgramaPeFalso(), referencias);

        ProgramaPe Nuevo() => new ProgramaPe
        {
            Programa = 901,
            PracticaEstrategia = 901
        };
        var nuevo = Nuevo();

        await servicio.Crear(nuevo);
        Verificador.Revisar((await servicio.ObtenerTodos(1000)).Count() == 1,
            "programa_pe: se crea con sus padres activos y se lista.",
            "programa_pe: debía haber exactamente un registro.");

        await Verificador.EsperarExcepcion<NoEncontradoExcepcion>(
            () => servicio.ObtenerPorLlave(999999, 999999),
            "programa_pe: una llave inexistente lanza NoEncontradoExcepcion (404).");

        await Verificador.EsperarExcepcion<LlaveDuplicadaExcepcion>(
            () => servicio.Crear(Nuevo()),
            "programa_pe: la llave repetida lanza LlaveDuplicadaExcepcion (409).");

        await Verificador.EsperarExcepcion<ReferenciaInvalidaExcepcion>(
            () => { var x = Nuevo(); x.Programa = 88888; return servicio.Crear(x); },
            "programa_pe: programa hacia un padre inexistente o retirado lanza ReferenciaInvalidaExcepcion (400).");

        await Verificador.EsperarExcepcion<ReferenciaInvalidaExcepcion>(
            () => { var x = Nuevo(); x.PracticaEstrategia = 88888; return servicio.Crear(x); },
            "programa_pe: practica_estrategia hacia un padre inexistente o retirado lanza ReferenciaInvalidaExcepcion (400).");

        await servicio.Eliminar(nuevo.Programa, nuevo.PracticaEstrategia);
        await Verificador.EsperarExcepcion<NoEncontradoExcepcion>(
            () => servicio.Eliminar(nuevo.Programa, nuevo.PracticaEstrategia),
            "programa_pe: el segundo retiro lanza NoEncontradoExcepcion (404).");
        Verificador.Revisar(!(await servicio.ObtenerTodos(1000)).Any(),
            "programa_pe: tras retirarlo, el listado vuelve a estar vacío.",
            "programa_pe: debía quedar vacío.");
    }

    private static async Task ProbarProgramaCi()
    {
        Console.WriteLine();
        Console.WriteLine("--- v2 · programa_ci ---");

        var referencias = new RepositorioReferenciasFalso();
        referencias.Programas.Add(901);
        referencias.CaracteristicasInnovacion.Add(901);
        IServicioProgramaCi servicio = new ServicioProgramaCi(new RepositorioProgramaCiFalso(), referencias);

        ProgramaCi Nuevo() => new ProgramaCi
        {
            Programa = 901,
            CarInnovacion = 901
        };
        var nuevo = Nuevo();

        await servicio.Crear(nuevo);
        Verificador.Revisar((await servicio.ObtenerTodos(1000)).Count() == 1,
            "programa_ci: se crea con sus padres activos y se lista.",
            "programa_ci: debía haber exactamente un registro.");

        await Verificador.EsperarExcepcion<NoEncontradoExcepcion>(
            () => servicio.ObtenerPorLlave(999999, 999999),
            "programa_ci: una llave inexistente lanza NoEncontradoExcepcion (404).");

        await Verificador.EsperarExcepcion<LlaveDuplicadaExcepcion>(
            () => servicio.Crear(Nuevo()),
            "programa_ci: la llave repetida lanza LlaveDuplicadaExcepcion (409).");

        await Verificador.EsperarExcepcion<ReferenciaInvalidaExcepcion>(
            () => { var x = Nuevo(); x.Programa = 88888; return servicio.Crear(x); },
            "programa_ci: programa hacia un padre inexistente o retirado lanza ReferenciaInvalidaExcepcion (400).");

        await Verificador.EsperarExcepcion<ReferenciaInvalidaExcepcion>(
            () => { var x = Nuevo(); x.CarInnovacion = 88888; return servicio.Crear(x); },
            "programa_ci: car_innovacion hacia un padre inexistente o retirado lanza ReferenciaInvalidaExcepcion (400).");

        await servicio.Eliminar(nuevo.Programa, nuevo.CarInnovacion);
        await Verificador.EsperarExcepcion<NoEncontradoExcepcion>(
            () => servicio.Eliminar(nuevo.Programa, nuevo.CarInnovacion),
            "programa_ci: el segundo retiro lanza NoEncontradoExcepcion (404).");
        Verificador.Revisar(!(await servicio.ObtenerTodos(1000)).Any(),
            "programa_ci: tras retirarlo, el listado vuelve a estar vacío.",
            "programa_ci: debía quedar vacío.");
    }

    private static async Task ProbarAnPrograma()
    {
        Console.WriteLine();
        Console.WriteLine("--- v2 · an_programa ---");

        var referencias = new RepositorioReferenciasFalso();
        referencias.AspectosNormativos.Add(901);
        referencias.Programas.Add(901);
        IServicioAnPrograma servicio = new ServicioAnPrograma(new RepositorioAnProgramaFalso(), referencias);

        AnPrograma Nuevo() => new AnPrograma
        {
            AspectoNormativo = 901,
            Programa = 901
        };
        var nuevo = Nuevo();

        await servicio.Crear(nuevo);
        Verificador.Revisar((await servicio.ObtenerTodos(1000)).Count() == 1,
            "an_programa: se crea con sus padres activos y se lista.",
            "an_programa: debía haber exactamente un registro.");

        await Verificador.EsperarExcepcion<NoEncontradoExcepcion>(
            () => servicio.ObtenerPorLlave(999999, 999999),
            "an_programa: una llave inexistente lanza NoEncontradoExcepcion (404).");

        await Verificador.EsperarExcepcion<LlaveDuplicadaExcepcion>(
            () => servicio.Crear(Nuevo()),
            "an_programa: la llave repetida lanza LlaveDuplicadaExcepcion (409).");

        await Verificador.EsperarExcepcion<ReferenciaInvalidaExcepcion>(
            () => { var x = Nuevo(); x.AspectoNormativo = 88888; return servicio.Crear(x); },
            "an_programa: aspecto_normativo hacia un padre inexistente o retirado lanza ReferenciaInvalidaExcepcion (400).");

        await Verificador.EsperarExcepcion<ReferenciaInvalidaExcepcion>(
            () => { var x = Nuevo(); x.Programa = 88888; return servicio.Crear(x); },
            "an_programa: programa hacia un padre inexistente o retirado lanza ReferenciaInvalidaExcepcion (400).");

        await servicio.Eliminar(nuevo.AspectoNormativo, nuevo.Programa);
        await Verificador.EsperarExcepcion<NoEncontradoExcepcion>(
            () => servicio.Eliminar(nuevo.AspectoNormativo, nuevo.Programa),
            "an_programa: el segundo retiro lanza NoEncontradoExcepcion (404).");
        Verificador.Revisar(!(await servicio.ObtenerTodos(1000)).Any(),
            "an_programa: tras retirarlo, el listado vuelve a estar vacío.",
            "an_programa: debía quedar vacío.");
    }

    private static async Task ProbarEnfoqueRc()
    {
        Console.WriteLine();
        Console.WriteLine("--- v2 · enfoque_rc ---");

        var referencias = new RepositorioReferenciasFalso();
        referencias.Enfoques.Add(901);
        referencias.RegistrosCalificados.Add(901);
        IServicioEnfoqueRc servicio = new ServicioEnfoqueRc(new RepositorioEnfoqueRcFalso(), referencias);

        EnfoqueRc Nuevo() => new EnfoqueRc
        {
            Enfoque = 901,
            RegistroCalificado = 901
        };
        var nuevo = Nuevo();

        await servicio.Crear(nuevo);
        Verificador.Revisar((await servicio.ObtenerTodos(1000)).Count() == 1,
            "enfoque_rc: se crea con sus padres activos y se lista.",
            "enfoque_rc: debía haber exactamente un registro.");

        await Verificador.EsperarExcepcion<NoEncontradoExcepcion>(
            () => servicio.ObtenerPorLlave(999999, 999999),
            "enfoque_rc: una llave inexistente lanza NoEncontradoExcepcion (404).");

        await Verificador.EsperarExcepcion<LlaveDuplicadaExcepcion>(
            () => servicio.Crear(Nuevo()),
            "enfoque_rc: la llave repetida lanza LlaveDuplicadaExcepcion (409).");

        await Verificador.EsperarExcepcion<ReferenciaInvalidaExcepcion>(
            () => { var x = Nuevo(); x.Enfoque = 88888; return servicio.Crear(x); },
            "enfoque_rc: enfoque hacia un padre inexistente o retirado lanza ReferenciaInvalidaExcepcion (400).");

        await Verificador.EsperarExcepcion<ReferenciaInvalidaExcepcion>(
            () => { var x = Nuevo(); x.RegistroCalificado = 88888; return servicio.Crear(x); },
            "enfoque_rc: registro_calificado hacia un padre inexistente o retirado lanza ReferenciaInvalidaExcepcion (400).");

        await servicio.Eliminar(nuevo.Enfoque, nuevo.RegistroCalificado);
        await Verificador.EsperarExcepcion<NoEncontradoExcepcion>(
            () => servicio.Eliminar(nuevo.Enfoque, nuevo.RegistroCalificado),
            "enfoque_rc: el segundo retiro lanza NoEncontradoExcepcion (404).");
        Verificador.Revisar(!(await servicio.ObtenerTodos(1000)).Any(),
            "enfoque_rc: tras retirarlo, el listado vuelve a estar vacío.",
            "enfoque_rc: debía quedar vacío.");
    }
}

/// <summary>El repositorio de mentiras de programa_ac: un diccionario en memoria.</summary>
public class RepositorioProgramaAcFalso : IRepositorioProgramaAc
{
    private readonly Dictionary<string, ProgramaAc> _filas = new();

    public Task<IEnumerable<ProgramaAc>> ObtenerTodos(int limite) =>
        Task.FromResult(_filas.Values.Take(limite).AsEnumerable());

    public Task<ProgramaAc?> ObtenerPorLlave(int programa, string areaConocimiento) =>
        Task.FromResult(_filas.TryGetValue($"{programa}|{areaConocimiento}", out var f) ? f : null);

    public Task Crear(ProgramaAc entidad)
    {
        var clave = $"{entidad.Programa}|{entidad.AreaConocimiento}";
        // Lo mismo que hace la PK del motor, traducido como lo traduce el
        // repositorio real.
        if (_filas.ContainsKey(clave)) throw new LlaveDuplicadaExcepcion("llave repetida");
        _filas[clave] = entidad;
        return Task.CompletedTask;
    }

    public Task<int> EliminarLogico(int programa, string areaConocimiento) =>
        Task.FromResult(_filas.Remove($"{programa}|{areaConocimiento}") ? 1 : 0);
}

/// <summary>El repositorio de mentiras de programa_pe: un diccionario en memoria.</summary>
public class RepositorioProgramaPeFalso : IRepositorioProgramaPe
{
    private readonly Dictionary<string, ProgramaPe> _filas = new();

    public Task<IEnumerable<ProgramaPe>> ObtenerTodos(int limite) =>
        Task.FromResult(_filas.Values.Take(limite).AsEnumerable());

    public Task<ProgramaPe?> ObtenerPorLlave(int programa, int practicaEstrategia) =>
        Task.FromResult(_filas.TryGetValue($"{programa}|{practicaEstrategia}", out var f) ? f : null);

    public Task Crear(ProgramaPe entidad)
    {
        var clave = $"{entidad.Programa}|{entidad.PracticaEstrategia}";
        // Lo mismo que hace la PK del motor, traducido como lo traduce el
        // repositorio real.
        if (_filas.ContainsKey(clave)) throw new LlaveDuplicadaExcepcion("llave repetida");
        _filas[clave] = entidad;
        return Task.CompletedTask;
    }

    public Task<int> EliminarLogico(int programa, int practicaEstrategia) =>
        Task.FromResult(_filas.Remove($"{programa}|{practicaEstrategia}") ? 1 : 0);
}

/// <summary>El repositorio de mentiras de programa_ci: un diccionario en memoria.</summary>
public class RepositorioProgramaCiFalso : IRepositorioProgramaCi
{
    private readonly Dictionary<string, ProgramaCi> _filas = new();

    public Task<IEnumerable<ProgramaCi>> ObtenerTodos(int limite) =>
        Task.FromResult(_filas.Values.Take(limite).AsEnumerable());

    public Task<ProgramaCi?> ObtenerPorLlave(int programa, int carInnovacion) =>
        Task.FromResult(_filas.TryGetValue($"{programa}|{carInnovacion}", out var f) ? f : null);

    public Task Crear(ProgramaCi entidad)
    {
        var clave = $"{entidad.Programa}|{entidad.CarInnovacion}";
        // Lo mismo que hace la PK del motor, traducido como lo traduce el
        // repositorio real.
        if (_filas.ContainsKey(clave)) throw new LlaveDuplicadaExcepcion("llave repetida");
        _filas[clave] = entidad;
        return Task.CompletedTask;
    }

    public Task<int> EliminarLogico(int programa, int carInnovacion) =>
        Task.FromResult(_filas.Remove($"{programa}|{carInnovacion}") ? 1 : 0);
}

/// <summary>El repositorio de mentiras de an_programa: un diccionario en memoria.</summary>
public class RepositorioAnProgramaFalso : IRepositorioAnPrograma
{
    private readonly Dictionary<string, AnPrograma> _filas = new();

    public Task<IEnumerable<AnPrograma>> ObtenerTodos(int limite) =>
        Task.FromResult(_filas.Values.Take(limite).AsEnumerable());

    public Task<AnPrograma?> ObtenerPorLlave(int aspectoNormativo, int programa) =>
        Task.FromResult(_filas.TryGetValue($"{aspectoNormativo}|{programa}", out var f) ? f : null);

    public Task Crear(AnPrograma entidad)
    {
        var clave = $"{entidad.AspectoNormativo}|{entidad.Programa}";
        // Lo mismo que hace la PK del motor, traducido como lo traduce el
        // repositorio real.
        if (_filas.ContainsKey(clave)) throw new LlaveDuplicadaExcepcion("llave repetida");
        _filas[clave] = entidad;
        return Task.CompletedTask;
    }

    public Task<int> EliminarLogico(int aspectoNormativo, int programa) =>
        Task.FromResult(_filas.Remove($"{aspectoNormativo}|{programa}") ? 1 : 0);
}

/// <summary>El repositorio de mentiras de enfoque_rc: un diccionario en memoria.</summary>
public class RepositorioEnfoqueRcFalso : IRepositorioEnfoqueRc
{
    private readonly Dictionary<string, EnfoqueRc> _filas = new();

    public Task<IEnumerable<EnfoqueRc>> ObtenerTodos(int limite) =>
        Task.FromResult(_filas.Values.Take(limite).AsEnumerable());

    public Task<EnfoqueRc?> ObtenerPorLlave(int enfoque, int registroCalificado) =>
        Task.FromResult(_filas.TryGetValue($"{enfoque}|{registroCalificado}", out var f) ? f : null);

    public Task Crear(EnfoqueRc entidad)
    {
        var clave = $"{entidad.Enfoque}|{entidad.RegistroCalificado}";
        // Lo mismo que hace la PK del motor, traducido como lo traduce el
        // repositorio real.
        if (_filas.ContainsKey(clave)) throw new LlaveDuplicadaExcepcion("llave repetida");
        _filas[clave] = entidad;
        return Task.CompletedTask;
    }

    public Task<int> EliminarLogico(int enfoque, int registroCalificado) =>
        Task.FromResult(_filas.Remove($"{enfoque}|{registroCalificado}") ? 1 : 0);
}
