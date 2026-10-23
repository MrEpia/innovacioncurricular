using ApiInnovacion.Excepciones;
using ApiInnovacion.Modelos;
using ApiInnovacion.Repositorios;
using ApiInnovacion.Servicios;

namespace ApiInnovacion.Pruebas;

/// <summary>
/// Prueba de capas del bloque «Actividades» de la v2: activ_academica, pasantia, premio, docente_departamento, alianza, aa_rc.
/// Responsable: David Epia. Solo su responsable edita este archivo.
///
/// Cada servicio corre contra su repositorio de mentiras y contra
/// RepositorioReferenciasFalso, donde cada prueba decide qué padres «existen»
/// y qué hijos «dependen»: así se comprueban las reglas de integridad sin SQL
/// Server.
/// </summary>
public static class PruebasActividades
{
    public static async Task Ejecutar()
    {
        await ProbarActivAcademica();
        await ProbarPasantia();
        await ProbarPremio();
        await ProbarDocenteDepartamento();
        await ProbarAlianza();
        await ProbarAaRc();
    }

    private static async Task ProbarActivAcademica()
    {
        Console.WriteLine();
        Console.WriteLine("--- v2 · activ_academica ---");

        var referencias = new RepositorioReferenciasFalso();
        referencias.Programas.Add(901);
        IServicioActivAcademica servicio = new ServicioActivAcademica(new RepositorioActivAcademicaFalso(), referencias);

        ActivAcademica Nuevo() => new ActivAcademica
        {
            Id = 101,
            Nombre = "Nombre",
            NumCreditos = 101,
            Tipo = "Tipo",
            AreaFormacion = "Área de formación",
            HAcom = 101,
            HIndep = 101,
            Idioma = "Idioma",
            Espejo = (byte)0,
            EntidadEspejo = "Entidad espejo",
            PaisEspejo = "País espejo",
            Disenio = 901
        };
        var nuevo = Nuevo();

        await servicio.Crear(nuevo);
        Verificador.Revisar((await servicio.ObtenerTodos(1000)).Count() == 1,
            "activ_academica: se crea con sus padres activos y se lista.",
            "activ_academica: debía haber exactamente un registro.");

        await Verificador.EsperarExcepcion<NoEncontradoExcepcion>(
            () => servicio.ObtenerPorId(999999),
            "activ_academica: una llave inexistente lanza NoEncontradoExcepcion (404).");

        await Verificador.EsperarExcepcion<LlaveDuplicadaExcepcion>(
            () => servicio.Crear(Nuevo()),
            "activ_academica: la llave repetida lanza LlaveDuplicadaExcepcion (409).");

        await Verificador.EsperarExcepcion<ReferenciaInvalidaExcepcion>(
            () => { var x = Nuevo(); x.Disenio = 88888; return servicio.Crear(x); },
            "activ_academica: disenio hacia un padre inexistente o retirado lanza ReferenciaInvalidaExcepcion (400).");

        await Verificador.EsperarExcepcion<ArgumentException>(
            () => { var x = Nuevo(); x.NumCreditos = -1; return servicio.Crear(x); },
            "activ_academica: numCreditos negativo lanza ArgumentException (400).");

        await Verificador.EsperarExcepcion<ArgumentException>(
            () => { var x = Nuevo(); x.HAcom = -1; return servicio.Crear(x); },
            "activ_academica: hAcom negativo lanza ArgumentException (400).");

        await Verificador.EsperarExcepcion<ArgumentException>(
            () => { var x = Nuevo(); x.HIndep = -1; return servicio.Crear(x); },
            "activ_academica: hIndep negativo lanza ArgumentException (400).");

        await Verificador.EsperarExcepcion<ArgumentException>(
            () => { var x = Nuevo(); x.Espejo = 5; return servicio.Crear(x); },
            "activ_academica: espejo distinto de 0 y 1 lanza ArgumentException (400).");

        await Verificador.EsperarExcepcion<ArgumentException>(
            () => servicio.ActualizarParcial(nuevo.Id, new ActivAcademicaCambios()),
            "activ_academica: un PATCH vacío lanza ArgumentException (400), no NoEncontrado.");

        await servicio.ActualizarParcial(nuevo.Id, new ActivAcademicaCambios { Nombre = "Cambiado" });
        Verificador.Revisar((await servicio.ObtenerPorId(nuevo.Id)).Nombre == "Cambiado",
            "activ_academica: el PATCH escribe solo lo enviado.",
            "activ_academica: el PATCH no escribió nombre.");

        referencias.Dependientes[$"activ_academica:{nuevo.Id}"] = new() { "hija (1)" };
        await Verificador.EsperarExcepcion<DependenciasActivasExcepcion>(
            () => servicio.Eliminar(nuevo.Id),
            "activ_academica: con hijos activos no se puede retirar (409).");
        referencias.Dependientes.Clear();

        await servicio.Eliminar(nuevo.Id);
        await Verificador.EsperarExcepcion<NoEncontradoExcepcion>(
            () => servicio.Eliminar(nuevo.Id),
            "activ_academica: el segundo retiro lanza NoEncontradoExcepcion (404).");
        Verificador.Revisar(!(await servicio.ObtenerTodos(1000)).Any(),
            "activ_academica: tras retirarlo, el listado vuelve a estar vacío.",
            "activ_academica: debía quedar vacío.");
    }

    private static async Task ProbarPasantia()
    {
        Console.WriteLine();
        Console.WriteLine("--- v2 · pasantia ---");

        var referencias = new RepositorioReferenciasFalso();
        referencias.Programas.Add(901);
        IServicioPasantia servicio = new ServicioPasantia(new RepositorioPasantiaFalso(), referencias);

        Pasantia Nuevo() => new Pasantia
        {
            Id = 101,
            Nombre = "Nombre",
            Pais = "País",
            Empresa = "Empresa",
            Descripcion = "Descripción",
            Programa = 901
        };
        var nuevo = Nuevo();

        await servicio.Crear(nuevo);
        Verificador.Revisar((await servicio.ObtenerTodos(1000)).Count() == 1,
            "pasantia: se crea con sus padres activos y se lista.",
            "pasantia: debía haber exactamente un registro.");

        await Verificador.EsperarExcepcion<NoEncontradoExcepcion>(
            () => servicio.ObtenerPorId(999999),
            "pasantia: una llave inexistente lanza NoEncontradoExcepcion (404).");

        await Verificador.EsperarExcepcion<LlaveDuplicadaExcepcion>(
            () => servicio.Crear(Nuevo()),
            "pasantia: la llave repetida lanza LlaveDuplicadaExcepcion (409).");

        await Verificador.EsperarExcepcion<ReferenciaInvalidaExcepcion>(
            () => { var x = Nuevo(); x.Programa = 88888; return servicio.Crear(x); },
            "pasantia: programa hacia un padre inexistente o retirado lanza ReferenciaInvalidaExcepcion (400).");

        await Verificador.EsperarExcepcion<ArgumentException>(
            () => servicio.ActualizarParcial(nuevo.Id, new PasantiaCambios()),
            "pasantia: un PATCH vacío lanza ArgumentException (400), no NoEncontrado.");

        await servicio.ActualizarParcial(nuevo.Id, new PasantiaCambios { Nombre = "Cambiado" });
        Verificador.Revisar((await servicio.ObtenerPorId(nuevo.Id)).Nombre == "Cambiado",
            "pasantia: el PATCH escribe solo lo enviado.",
            "pasantia: el PATCH no escribió nombre.");

        await servicio.Eliminar(nuevo.Id);
        await Verificador.EsperarExcepcion<NoEncontradoExcepcion>(
            () => servicio.Eliminar(nuevo.Id),
            "pasantia: el segundo retiro lanza NoEncontradoExcepcion (404).");
        Verificador.Revisar(!(await servicio.ObtenerTodos(1000)).Any(),
            "pasantia: tras retirarlo, el listado vuelve a estar vacío.",
            "pasantia: debía quedar vacío.");
    }

    private static async Task ProbarPremio()
    {
        Console.WriteLine();
        Console.WriteLine("--- v2 · premio ---");

        var referencias = new RepositorioReferenciasFalso();
        referencias.Programas.Add(901);
        IServicioPremio servicio = new ServicioPremio(new RepositorioPremioFalso(), referencias);

        Premio Nuevo() => new Premio
        {
            Id = 101,
            Nombre = "Nombre",
            Descripcion = "Descripción",
            Fecha = new DateTime(2024, 2, 1),
            EntidadOtorgante = "Entidad otorgante",
            Pais = "País",
            Programa = 901
        };
        var nuevo = Nuevo();

        await servicio.Crear(nuevo);
        Verificador.Revisar((await servicio.ObtenerTodos(1000)).Count() == 1,
            "premio: se crea con sus padres activos y se lista.",
            "premio: debía haber exactamente un registro.");

        await Verificador.EsperarExcepcion<NoEncontradoExcepcion>(
            () => servicio.ObtenerPorId(999999),
            "premio: una llave inexistente lanza NoEncontradoExcepcion (404).");

        await Verificador.EsperarExcepcion<LlaveDuplicadaExcepcion>(
            () => servicio.Crear(Nuevo()),
            "premio: la llave repetida lanza LlaveDuplicadaExcepcion (409).");

        await Verificador.EsperarExcepcion<ReferenciaInvalidaExcepcion>(
            () => { var x = Nuevo(); x.Programa = 88888; return servicio.Crear(x); },
            "premio: programa hacia un padre inexistente o retirado lanza ReferenciaInvalidaExcepcion (400).");

        await Verificador.EsperarExcepcion<ArgumentException>(
            () => servicio.ActualizarParcial(nuevo.Id, new PremioCambios()),
            "premio: un PATCH vacío lanza ArgumentException (400), no NoEncontrado.");

        await servicio.ActualizarParcial(nuevo.Id, new PremioCambios { Nombre = "Cambiado" });
        Verificador.Revisar((await servicio.ObtenerPorId(nuevo.Id)).Nombre == "Cambiado",
            "premio: el PATCH escribe solo lo enviado.",
            "premio: el PATCH no escribió nombre.");

        await servicio.Eliminar(nuevo.Id);
        await Verificador.EsperarExcepcion<NoEncontradoExcepcion>(
            () => servicio.Eliminar(nuevo.Id),
            "premio: el segundo retiro lanza NoEncontradoExcepcion (404).");
        Verificador.Revisar(!(await servicio.ObtenerTodos(1000)).Any(),
            "premio: tras retirarlo, el listado vuelve a estar vacío.",
            "premio: debía quedar vacío.");
    }

    private static async Task ProbarDocenteDepartamento()
    {
        Console.WriteLine();
        Console.WriteLine("--- v2 · docente_departamento ---");

        var referencias = new RepositorioReferenciasFalso();
        referencias.Programas.Add(901);
        IServicioDocenteDepartamento servicio = new ServicioDocenteDepartamento(new RepositorioDocenteDepartamentoFalso(), referencias);

        DocenteDepartamento Nuevo() => new DocenteDepartamento
        {
            Docente = 101,
            Departamento = 901,
            Dedicacion = "Tiempo Completo",
            Modalidad = "Modalidad",
            FechaIngreso = new DateTime(2024, 2, 1),
            FechaSalida = new DateTime(2024, 2, 1)
        };
        var nuevo = Nuevo();

        await servicio.Crear(nuevo);
        Verificador.Revisar((await servicio.ObtenerTodos(1000)).Count() == 1,
            "docente_departamento: se crea con sus padres activos y se lista.",
            "docente_departamento: debía haber exactamente un registro.");

        await Verificador.EsperarExcepcion<NoEncontradoExcepcion>(
            () => servicio.ObtenerPorLlave(999999, 999999),
            "docente_departamento: una llave inexistente lanza NoEncontradoExcepcion (404).");

        await Verificador.EsperarExcepcion<LlaveDuplicadaExcepcion>(
            () => servicio.Crear(Nuevo()),
            "docente_departamento: la llave repetida lanza LlaveDuplicadaExcepcion (409).");

        await Verificador.EsperarExcepcion<ReferenciaInvalidaExcepcion>(
            () => { var x = Nuevo(); x.Departamento = 88888; return servicio.Crear(x); },
            "docente_departamento: departamento hacia un padre inexistente o retirado lanza ReferenciaInvalidaExcepcion (400).");

        await Verificador.EsperarExcepcion<ArgumentException>(
            () => { var x = Nuevo(); x.Dedicacion = "Ocasional"; return servicio.Crear(x); },
            "docente_departamento: una dedicación fuera de la lista lanza ArgumentException (400).");

        await Verificador.EsperarExcepcion<ArgumentException>(
            () => { var x = Nuevo(); x.FechaIngreso = new DateTime(2024, 3, 1); x.FechaSalida = new DateTime(2024, 1, 1); return servicio.Crear(x); },
            "docente_departamento: fechaSalida anterior a fechaIngreso lanza ArgumentException (400).");

        await Verificador.EsperarExcepcion<ArgumentException>(
            () => servicio.ActualizarParcial(nuevo.Docente, nuevo.Departamento, new DocenteDepartamentoCambios()),
            "docente_departamento: un PATCH vacío lanza ArgumentException (400), no NoEncontrado.");

        await servicio.ActualizarParcial(nuevo.Docente, nuevo.Departamento, new DocenteDepartamentoCambios { Modalidad = "Cambiado" });
        Verificador.Revisar((await servicio.ObtenerPorLlave(nuevo.Docente, nuevo.Departamento)).Modalidad == "Cambiado",
            "docente_departamento: el PATCH escribe solo lo enviado.",
            "docente_departamento: el PATCH no escribió modalidad.");

        await Verificador.EsperarExcepcion<ArgumentException>(
            () => servicio.ActualizarParcial(nuevo.Docente, nuevo.Departamento, new DocenteDepartamentoCambios { FechaSalida = new DateTime(2000, 1, 1) }),
            "docente_departamento: un PATCH que solo trae fechaSalida se valida contra el fechaIngreso guardado.");

        await servicio.Eliminar(nuevo.Docente, nuevo.Departamento);
        await Verificador.EsperarExcepcion<NoEncontradoExcepcion>(
            () => servicio.Eliminar(nuevo.Docente, nuevo.Departamento),
            "docente_departamento: el segundo retiro lanza NoEncontradoExcepcion (404).");
        Verificador.Revisar(!(await servicio.ObtenerTodos(1000)).Any(),
            "docente_departamento: tras retirarlo, el listado vuelve a estar vacío.",
            "docente_departamento: debía quedar vacío.");
    }

    private static async Task ProbarAlianza()
    {
        Console.WriteLine();
        Console.WriteLine("--- v2 · alianza ---");

        var referencias = new RepositorioReferenciasFalso();
        referencias.Aliados.Add(901);
        referencias.Programas.Add(901);
        IServicioAlianza servicio = new ServicioAlianza(new RepositorioAlianzaFalso(), referencias);

        Alianza Nuevo() => new Alianza
        {
            Aliado = 901,
            Departamento = 901,
            FechaInicio = new DateTime(2024, 2, 1),
            FechaFin = new DateTime(2024, 2, 1),
            Docente = 101
        };
        var nuevo = Nuevo();

        await servicio.Crear(nuevo);
        Verificador.Revisar((await servicio.ObtenerTodos(1000)).Count() == 1,
            "alianza: se crea con sus padres activos y se lista.",
            "alianza: debía haber exactamente un registro.");

        await Verificador.EsperarExcepcion<NoEncontradoExcepcion>(
            () => servicio.ObtenerPorLlave(999999, 999999),
            "alianza: una llave inexistente lanza NoEncontradoExcepcion (404).");

        await Verificador.EsperarExcepcion<LlaveDuplicadaExcepcion>(
            () => servicio.Crear(Nuevo()),
            "alianza: la llave repetida lanza LlaveDuplicadaExcepcion (409).");

        await Verificador.EsperarExcepcion<ReferenciaInvalidaExcepcion>(
            () => { var x = Nuevo(); x.Aliado = 88888; return servicio.Crear(x); },
            "alianza: aliado hacia un padre inexistente o retirado lanza ReferenciaInvalidaExcepcion (400).");

        await Verificador.EsperarExcepcion<ReferenciaInvalidaExcepcion>(
            () => { var x = Nuevo(); x.Departamento = 88888; return servicio.Crear(x); },
            "alianza: departamento hacia un padre inexistente o retirado lanza ReferenciaInvalidaExcepcion (400).");

        await Verificador.EsperarExcepcion<ArgumentException>(
            () => { var x = Nuevo(); x.FechaInicio = new DateTime(2024, 3, 1); x.FechaFin = new DateTime(2024, 1, 1); return servicio.Crear(x); },
            "alianza: fechaFin anterior a fechaInicio lanza ArgumentException (400).");

        await Verificador.EsperarExcepcion<ArgumentException>(
            () => servicio.ActualizarParcial(nuevo.Aliado, nuevo.Departamento, new AlianzaCambios()),
            "alianza: un PATCH vacío lanza ArgumentException (400), no NoEncontrado.");

        await Verificador.EsperarExcepcion<ArgumentException>(
            () => servicio.ActualizarParcial(nuevo.Aliado, nuevo.Departamento, new AlianzaCambios { FechaFin = new DateTime(2000, 1, 1) }),
            "alianza: un PATCH que solo trae fechaFin se valida contra el fechaInicio guardado.");

        await servicio.Eliminar(nuevo.Aliado, nuevo.Departamento);
        await Verificador.EsperarExcepcion<NoEncontradoExcepcion>(
            () => servicio.Eliminar(nuevo.Aliado, nuevo.Departamento),
            "alianza: el segundo retiro lanza NoEncontradoExcepcion (404).");
        Verificador.Revisar(!(await servicio.ObtenerTodos(1000)).Any(),
            "alianza: tras retirarlo, el listado vuelve a estar vacío.",
            "alianza: debía quedar vacío.");
    }

    private static async Task ProbarAaRc()
    {
        Console.WriteLine();
        Console.WriteLine("--- v2 · aa_rc ---");

        var referencias = new RepositorioReferenciasFalso();
        referencias.ActividadesAcademicas.Add(901);
        referencias.RegistrosCalificados.Add(901);
        IServicioAaRc servicio = new ServicioAaRc(new RepositorioAaRcFalso(), referencias);

        AaRc Nuevo() => new AaRc
        {
            ActivAcademicasIdcurso = 901,
            RegistroCalificadoCodigo = 901,
            Componente = "Componente",
            Semestre = "Semestre"
        };
        var nuevo = Nuevo();

        await servicio.Crear(nuevo);
        Verificador.Revisar((await servicio.ObtenerTodos(1000)).Count() == 1,
            "aa_rc: se crea con sus padres activos y se lista.",
            "aa_rc: debía haber exactamente un registro.");

        await Verificador.EsperarExcepcion<NoEncontradoExcepcion>(
            () => servicio.ObtenerPorLlave(999999, 999999),
            "aa_rc: una llave inexistente lanza NoEncontradoExcepcion (404).");

        await Verificador.EsperarExcepcion<LlaveDuplicadaExcepcion>(
            () => servicio.Crear(Nuevo()),
            "aa_rc: la llave repetida lanza LlaveDuplicadaExcepcion (409).");

        await Verificador.EsperarExcepcion<ReferenciaInvalidaExcepcion>(
            () => { var x = Nuevo(); x.ActivAcademicasIdcurso = 88888; return servicio.Crear(x); },
            "aa_rc: activ_academicas_idcurso hacia un padre inexistente o retirado lanza ReferenciaInvalidaExcepcion (400).");

        await Verificador.EsperarExcepcion<ReferenciaInvalidaExcepcion>(
            () => { var x = Nuevo(); x.RegistroCalificadoCodigo = 88888; return servicio.Crear(x); },
            "aa_rc: registro_calificado_codigo hacia un padre inexistente o retirado lanza ReferenciaInvalidaExcepcion (400).");

        await Verificador.EsperarExcepcion<ArgumentException>(
            () => servicio.ActualizarParcial(nuevo.ActivAcademicasIdcurso, nuevo.RegistroCalificadoCodigo, new AaRcCambios()),
            "aa_rc: un PATCH vacío lanza ArgumentException (400), no NoEncontrado.");

        await servicio.ActualizarParcial(nuevo.ActivAcademicasIdcurso, nuevo.RegistroCalificadoCodigo, new AaRcCambios { Componente = "Cambiado" });
        Verificador.Revisar((await servicio.ObtenerPorLlave(nuevo.ActivAcademicasIdcurso, nuevo.RegistroCalificadoCodigo)).Componente == "Cambiado",
            "aa_rc: el PATCH escribe solo lo enviado.",
            "aa_rc: el PATCH no escribió componente.");

        await servicio.Eliminar(nuevo.ActivAcademicasIdcurso, nuevo.RegistroCalificadoCodigo);
        await Verificador.EsperarExcepcion<NoEncontradoExcepcion>(
            () => servicio.Eliminar(nuevo.ActivAcademicasIdcurso, nuevo.RegistroCalificadoCodigo),
            "aa_rc: el segundo retiro lanza NoEncontradoExcepcion (404).");
        Verificador.Revisar(!(await servicio.ObtenerTodos(1000)).Any(),
            "aa_rc: tras retirarlo, el listado vuelve a estar vacío.",
            "aa_rc: debía quedar vacío.");
    }
}

/// <summary>El repositorio de mentiras de activ_academica: un diccionario en memoria.</summary>
public class RepositorioActivAcademicaFalso : IRepositorioActivAcademica
{
    private readonly Dictionary<string, ActivAcademica> _filas = new();

    public Task<IEnumerable<ActivAcademica>> ObtenerTodos(int limite) =>
        Task.FromResult(_filas.Values.Take(limite).AsEnumerable());

    public Task<ActivAcademica?> ObtenerPorId(int id) =>
        Task.FromResult(_filas.TryGetValue($"{id}", out var f) ? f : null);

    public Task Crear(ActivAcademica entidad)
    {
        var clave = $"{entidad.Id}";
        // Lo mismo que hace la PK del motor, traducido como lo traduce el
        // repositorio real.
        if (_filas.ContainsKey(clave)) throw new LlaveDuplicadaExcepcion("llave repetida");
        _filas[clave] = entidad;
        return Task.CompletedTask;
    }

    public Task<int> Reemplazar(ActivAcademica entidad)
    {
        var clave = $"{entidad.Id}";
        if (!_filas.ContainsKey(clave)) return Task.FromResult(0);
        _filas[clave] = entidad;
        return Task.FromResult(1);
    }

    public Task<int> ActualizarParcial(int id, ActivAcademicaCambios cambios)
    {
        if (!_filas.TryGetValue($"{id}", out var f)) return Task.FromResult(0);
        if (cambios.Nombre != null) f.Nombre = cambios.Nombre;
        if (cambios.NumCreditos != null) f.NumCreditos = cambios.NumCreditos.Value;
        if (cambios.Tipo != null) f.Tipo = cambios.Tipo;
        if (cambios.AreaFormacion != null) f.AreaFormacion = cambios.AreaFormacion;
        if (cambios.HAcom != null) f.HAcom = cambios.HAcom.Value;
        if (cambios.HIndep != null) f.HIndep = cambios.HIndep.Value;
        if (cambios.Idioma != null) f.Idioma = cambios.Idioma;
        if (cambios.Espejo != null) f.Espejo = cambios.Espejo.Value;
        if (cambios.EntidadEspejo != null) f.EntidadEspejo = cambios.EntidadEspejo;
        if (cambios.PaisEspejo != null) f.PaisEspejo = cambios.PaisEspejo;
        if (cambios.Disenio != null) f.Disenio = cambios.Disenio;
        return Task.FromResult(1);
    }

    public Task<int> EliminarLogico(int id) =>
        Task.FromResult(_filas.Remove($"{id}") ? 1 : 0);
}

/// <summary>El repositorio de mentiras de pasantia: un diccionario en memoria.</summary>
public class RepositorioPasantiaFalso : IRepositorioPasantia
{
    private readonly Dictionary<string, Pasantia> _filas = new();

    public Task<IEnumerable<Pasantia>> ObtenerTodos(int limite) =>
        Task.FromResult(_filas.Values.Take(limite).AsEnumerable());

    public Task<Pasantia?> ObtenerPorId(int id) =>
        Task.FromResult(_filas.TryGetValue($"{id}", out var f) ? f : null);

    public Task Crear(Pasantia entidad)
    {
        var clave = $"{entidad.Id}";
        // Lo mismo que hace la PK del motor, traducido como lo traduce el
        // repositorio real.
        if (_filas.ContainsKey(clave)) throw new LlaveDuplicadaExcepcion("llave repetida");
        _filas[clave] = entidad;
        return Task.CompletedTask;
    }

    public Task<int> Reemplazar(Pasantia entidad)
    {
        var clave = $"{entidad.Id}";
        if (!_filas.ContainsKey(clave)) return Task.FromResult(0);
        _filas[clave] = entidad;
        return Task.FromResult(1);
    }

    public Task<int> ActualizarParcial(int id, PasantiaCambios cambios)
    {
        if (!_filas.TryGetValue($"{id}", out var f)) return Task.FromResult(0);
        if (cambios.Nombre != null) f.Nombre = cambios.Nombre;
        if (cambios.Pais != null) f.Pais = cambios.Pais;
        if (cambios.Empresa != null) f.Empresa = cambios.Empresa;
        if (cambios.Descripcion != null) f.Descripcion = cambios.Descripcion;
        if (cambios.Programa != null) f.Programa = cambios.Programa.Value;
        return Task.FromResult(1);
    }

    public Task<int> EliminarLogico(int id) =>
        Task.FromResult(_filas.Remove($"{id}") ? 1 : 0);
}

/// <summary>El repositorio de mentiras de premio: un diccionario en memoria.</summary>
public class RepositorioPremioFalso : IRepositorioPremio
{
    private readonly Dictionary<string, Premio> _filas = new();

    public Task<IEnumerable<Premio>> ObtenerTodos(int limite) =>
        Task.FromResult(_filas.Values.Take(limite).AsEnumerable());

    public Task<Premio?> ObtenerPorId(int id) =>
        Task.FromResult(_filas.TryGetValue($"{id}", out var f) ? f : null);

    public Task Crear(Premio entidad)
    {
        var clave = $"{entidad.Id}";
        // Lo mismo que hace la PK del motor, traducido como lo traduce el
        // repositorio real.
        if (_filas.ContainsKey(clave)) throw new LlaveDuplicadaExcepcion("llave repetida");
        _filas[clave] = entidad;
        return Task.CompletedTask;
    }

    public Task<int> Reemplazar(Premio entidad)
    {
        var clave = $"{entidad.Id}";
        if (!_filas.ContainsKey(clave)) return Task.FromResult(0);
        _filas[clave] = entidad;
        return Task.FromResult(1);
    }

    public Task<int> ActualizarParcial(int id, PremioCambios cambios)
    {
        if (!_filas.TryGetValue($"{id}", out var f)) return Task.FromResult(0);
        if (cambios.Nombre != null) f.Nombre = cambios.Nombre;
        if (cambios.Descripcion != null) f.Descripcion = cambios.Descripcion;
        if (cambios.Fecha != null) f.Fecha = cambios.Fecha.Value;
        if (cambios.EntidadOtorgante != null) f.EntidadOtorgante = cambios.EntidadOtorgante;
        if (cambios.Pais != null) f.Pais = cambios.Pais;
        if (cambios.Programa != null) f.Programa = cambios.Programa.Value;
        return Task.FromResult(1);
    }

    public Task<int> EliminarLogico(int id) =>
        Task.FromResult(_filas.Remove($"{id}") ? 1 : 0);
}

/// <summary>El repositorio de mentiras de docente_departamento: un diccionario en memoria.</summary>
public class RepositorioDocenteDepartamentoFalso : IRepositorioDocenteDepartamento
{
    private readonly Dictionary<string, DocenteDepartamento> _filas = new();

    public Task<IEnumerable<DocenteDepartamento>> ObtenerTodos(int limite) =>
        Task.FromResult(_filas.Values.Take(limite).AsEnumerable());

    public Task<DocenteDepartamento?> ObtenerPorLlave(int docente, int departamento) =>
        Task.FromResult(_filas.TryGetValue($"{docente}|{departamento}", out var f) ? f : null);

    public Task Crear(DocenteDepartamento entidad)
    {
        var clave = $"{entidad.Docente}|{entidad.Departamento}";
        // Lo mismo que hace la PK del motor, traducido como lo traduce el
        // repositorio real.
        if (_filas.ContainsKey(clave)) throw new LlaveDuplicadaExcepcion("llave repetida");
        _filas[clave] = entidad;
        return Task.CompletedTask;
    }

    public Task<int> Reemplazar(DocenteDepartamento entidad)
    {
        var clave = $"{entidad.Docente}|{entidad.Departamento}";
        if (!_filas.ContainsKey(clave)) return Task.FromResult(0);
        _filas[clave] = entidad;
        return Task.FromResult(1);
    }

    public Task<int> ActualizarParcial(int docente, int departamento, DocenteDepartamentoCambios cambios)
    {
        if (!_filas.TryGetValue($"{docente}|{departamento}", out var f)) return Task.FromResult(0);
        if (cambios.Dedicacion != null) f.Dedicacion = cambios.Dedicacion;
        if (cambios.Modalidad != null) f.Modalidad = cambios.Modalidad;
        if (cambios.FechaIngreso != null) f.FechaIngreso = cambios.FechaIngreso.Value;
        if (cambios.FechaSalida != null) f.FechaSalida = cambios.FechaSalida;
        return Task.FromResult(1);
    }

    public Task<int> EliminarLogico(int docente, int departamento) =>
        Task.FromResult(_filas.Remove($"{docente}|{departamento}") ? 1 : 0);
}

/// <summary>El repositorio de mentiras de alianza: un diccionario en memoria.</summary>
public class RepositorioAlianzaFalso : IRepositorioAlianza
{
    private readonly Dictionary<string, Alianza> _filas = new();

    public Task<IEnumerable<Alianza>> ObtenerTodos(int limite) =>
        Task.FromResult(_filas.Values.Take(limite).AsEnumerable());

    public Task<Alianza?> ObtenerPorLlave(int aliado, int departamento) =>
        Task.FromResult(_filas.TryGetValue($"{aliado}|{departamento}", out var f) ? f : null);

    public Task Crear(Alianza entidad)
    {
        var clave = $"{entidad.Aliado}|{entidad.Departamento}";
        // Lo mismo que hace la PK del motor, traducido como lo traduce el
        // repositorio real.
        if (_filas.ContainsKey(clave)) throw new LlaveDuplicadaExcepcion("llave repetida");
        _filas[clave] = entidad;
        return Task.CompletedTask;
    }

    public Task<int> Reemplazar(Alianza entidad)
    {
        var clave = $"{entidad.Aliado}|{entidad.Departamento}";
        if (!_filas.ContainsKey(clave)) return Task.FromResult(0);
        _filas[clave] = entidad;
        return Task.FromResult(1);
    }

    public Task<int> ActualizarParcial(int aliado, int departamento, AlianzaCambios cambios)
    {
        if (!_filas.TryGetValue($"{aliado}|{departamento}", out var f)) return Task.FromResult(0);
        if (cambios.FechaInicio != null) f.FechaInicio = cambios.FechaInicio.Value;
        if (cambios.FechaFin != null) f.FechaFin = cambios.FechaFin;
        if (cambios.Docente != null) f.Docente = cambios.Docente;
        return Task.FromResult(1);
    }

    public Task<int> EliminarLogico(int aliado, int departamento) =>
        Task.FromResult(_filas.Remove($"{aliado}|{departamento}") ? 1 : 0);
}

/// <summary>El repositorio de mentiras de aa_rc: un diccionario en memoria.</summary>
public class RepositorioAaRcFalso : IRepositorioAaRc
{
    private readonly Dictionary<string, AaRc> _filas = new();

    public Task<IEnumerable<AaRc>> ObtenerTodos(int limite) =>
        Task.FromResult(_filas.Values.Take(limite).AsEnumerable());

    public Task<AaRc?> ObtenerPorLlave(int activAcademicasIdcurso, int registroCalificadoCodigo) =>
        Task.FromResult(_filas.TryGetValue($"{activAcademicasIdcurso}|{registroCalificadoCodigo}", out var f) ? f : null);

    public Task Crear(AaRc entidad)
    {
        var clave = $"{entidad.ActivAcademicasIdcurso}|{entidad.RegistroCalificadoCodigo}";
        // Lo mismo que hace la PK del motor, traducido como lo traduce el
        // repositorio real.
        if (_filas.ContainsKey(clave)) throw new LlaveDuplicadaExcepcion("llave repetida");
        _filas[clave] = entidad;
        return Task.CompletedTask;
    }

    public Task<int> Reemplazar(AaRc entidad)
    {
        var clave = $"{entidad.ActivAcademicasIdcurso}|{entidad.RegistroCalificadoCodigo}";
        if (!_filas.ContainsKey(clave)) return Task.FromResult(0);
        _filas[clave] = entidad;
        return Task.FromResult(1);
    }

    public Task<int> ActualizarParcial(int activAcademicasIdcurso, int registroCalificadoCodigo, AaRcCambios cambios)
    {
        if (!_filas.TryGetValue($"{activAcademicasIdcurso}|{registroCalificadoCodigo}", out var f)) return Task.FromResult(0);
        if (cambios.Componente != null) f.Componente = cambios.Componente;
        if (cambios.Semestre != null) f.Semestre = cambios.Semestre;
        return Task.FromResult(1);
    }

    public Task<int> EliminarLogico(int activAcademicasIdcurso, int registroCalificadoCodigo) =>
        Task.FromResult(_filas.Remove($"{activAcademicasIdcurso}|{registroCalificadoCodigo}") ? 1 : 0);
}
