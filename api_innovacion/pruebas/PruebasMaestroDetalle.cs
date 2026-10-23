using ApiInnovacion.Excepciones;
using ApiInnovacion.Modelos;
using ApiInnovacion.Repositorios;
using ApiInnovacion.Servicios;

namespace ApiInnovacion.Pruebas;

/// <summary>
/// Prueba de capas del MAESTRO-DETALLE (registro_calificado y programa con sus
/// hijos), sin base de datos.
///
/// Lo que prueba aquí es la capa 2: que valide el maestro y CADA hijo antes de
/// llamar al repositorio, y que ante cualquier error no llame al repositorio
/// (no se guarda nada). El todo-o-nada DENTRO de la base lo prueba
/// humo_maestro_detalle.py contra el procedimiento real.
/// </summary>
public static class PruebasMaestroDetalle
{
    public static async Task Ejecutar()
    {
        Console.WriteLine();
        Console.WriteLine("========== Maestro-detalle ==========");
        await ProbarRegistroCalificadoCompleto();
        await ProbarProgramaCompleto();
    }

    private static async Task ProbarRegistroCalificadoCompleto()
    {
        Console.WriteLine();
        Console.WriteLine("--- registro_calificado con enfoques y actividades ---");

        var referencias = new RepositorioReferenciasFalso();
        referencias.Programas.Add(901);
        referencias.Enfoques.UnionWith(new[] { 1, 2 });
        referencias.ActividadesAcademicas.UnionWith(new[] { 7, 8 });
        var repositorio = new RepositorioRegistroCalificadoCompletoFalso();
        IServicioRegistroCalificadoCompleto servicio = new ServicioRegistroCalificadoCompleto(repositorio, referencias);

        RegistroCalificadoCompleto Nuevo(int codigo = 101) => new()
        {
            Codigo = codigo,
            CantCreditos = "160",
            HoraAcom = "48",
            HoraInd = "96",
            Metodologia = "Presencial",
            FechaInicio = new DateTime(2024, 2, 1),
            FechaFin = new DateTime(2031, 2, 1),
            DuracionAnios = "5",
            DuracionSemestres = "10",
            TipoTitulacion = "Profesional",
            Programa = 901,
            Enfoques = new() { new() { Enfoque = 1 }, new() { Enfoque = 2 } },
            Actividades = new() { new() { ActivAcademicasIdcurso = 7, Componente = "Básico", Semestre = "1" } }
        };

        await servicio.Crear(Nuevo());
        var leido = await servicio.ObtenerCompleto(101);
        Verificador.Revisar(leido.Enfoques.Count == 2 && leido.Actividades.Count == 1,
            "registro completo: se guarda el maestro con sus 2 enfoques y su actividad, y se lee junto.",
            "registro completo: debía leerse con 2 enfoques y 1 actividad.");

        await Verificador.EsperarExcepcion<NoEncontradoExcepcion>(
            () => servicio.ObtenerCompleto(999999),
            "registro completo: un código inexistente lanza NoEncontradoExcepcion (404).");

        await Verificador.EsperarExcepcion<LlaveDuplicadaExcepcion>(
            () => servicio.Crear(Nuevo()),
            "registro completo: la llave repetida lanza LlaveDuplicadaExcepcion (409).");

        var sinDetalle = Nuevo(102);
        sinDetalle.Enfoques.Clear();
        sinDetalle.Actividades.Clear();
        await Verificador.EsperarExcepcion<ArgumentException>(
            () => servicio.Crear(sinDetalle),
            "registro completo: sin ningún hijo es 400 (para eso está el POST normal).");

        var programaRetirado = Nuevo(103);
        programaRetirado.Programa = 555;
        await Verificador.EsperarExcepcion<ReferenciaInvalidaExcepcion>(
            () => servicio.Crear(programaRetirado),
            "registro completo: un programa inexistente o retirado es 400.");

        var fechas = Nuevo(104);
        fechas.FechaFin = new DateTime(2020, 1, 1);
        await Verificador.EsperarExcepcion<ArgumentException>(
            () => servicio.Crear(fechas),
            "registro completo: fechaFin anterior a fechaInicio es 400 (la regla del maestro se conserva).");

        var enfoqueMalo = Nuevo(105);
        enfoqueMalo.Enfoques.Add(new() { Enfoque = 99 });
        await Verificador.EsperarExcepcion<ReferenciaInvalidaExcepcion>(
            () => servicio.Crear(enfoqueMalo),
            "registro completo: un enfoque retirado en el detalle es 400.");

        var actividadMala = Nuevo(106);
        actividadMala.Actividades.Add(new() { ActivAcademicasIdcurso = 99, Componente = "X", Semestre = "2" });
        await Verificador.EsperarExcepcion<ReferenciaInvalidaExcepcion>(
            () => servicio.Crear(actividadMala),
            "registro completo: la SEGUNDA actividad inválida también se detecta (400).");

        var repetido = Nuevo(107);
        repetido.Enfoques.Add(new() { Enfoque = 1 });
        await Verificador.EsperarExcepcion<ArgumentException>(
            () => servicio.Crear(repetido),
            "registro completo: un enfoque repetido en la lista es 400.");

        Verificador.Revisar(repositorio.Guardados.Count == 1,
            "registro completo: ninguno de los intentos fallidos llegó al repositorio (no se guardó nada).",
            $"registro completo: el repositorio debía tener 1 registro y tiene {repositorio.Guardados.Count}.");
    }

    private static async Task ProbarProgramaCompleto()
    {
        Console.WriteLine();
        Console.WriteLine("--- programa con áreas, prácticas y características ---");

        var referencias = new RepositorioReferenciasFalso();
        referencias.Facultades.Add(10);
        referencias.AreasConocimiento.UnionWith(new[] { "1A01", "1A02" });
        referencias.PracticasEstrategias.Add(3);
        referencias.CaracteristicasInnovacion.Add(4);
        var repositorio = new RepositorioProgramaCompletoFalso();
        IServicioProgramaCompleto servicio = new ServicioProgramaCompleto(repositorio, referencias);

        ProgramaCompleto Nuevo(int id = 501) => new()
        {
            Id = id,
            Nombre = "Ingeniería de Sistemas",
            Tipo = "Pregrado",
            Nivel = "Profesional",
            FechaCreacion = "2001",
            NumeroCohortes = "20",
            CantGraduados = "500",
            FechaActualizacion = "2024",
            Ciudad = "Medellín",
            Facultad = 10,
            AreasConocimiento = new() { new() { AreaConocimiento = "1A01" }, new() { AreaConocimiento = "1A02" } },
            PracticasEstrategias = new() { new() { PracticaEstrategia = 3 } },
            CaracteristicasInnovacion = new() { new() { CarInnovacion = 4 } }
        };

        await servicio.Crear(Nuevo());
        var leido = await servicio.ObtenerCompleto(501);
        Verificador.Revisar(leido.AreasConocimiento.Count == 2 && leido.PracticasEstrategias.Count == 1
                            && leido.CaracteristicasInnovacion.Count == 1,
            "programa completo: se guarda con sus 2 áreas, 1 práctica y 1 característica.",
            "programa completo: el detalle leído no coincide con el guardado.");

        await Verificador.EsperarExcepcion<NoEncontradoExcepcion>(
            () => servicio.ObtenerCompleto(999999),
            "programa completo: un id inexistente lanza NoEncontradoExcepcion (404).");

        await Verificador.EsperarExcepcion<LlaveDuplicadaExcepcion>(
            () => servicio.Crear(Nuevo()),
            "programa completo: la llave repetida es 409.");

        var vacio = Nuevo(502);
        vacio.AreasConocimiento.Clear();
        vacio.PracticasEstrategias.Clear();
        vacio.CaracteristicasInnovacion.Clear();
        await Verificador.EsperarExcepcion<ArgumentException>(
            () => servicio.Crear(vacio),
            "programa completo: sin ningún hijo es 400.");

        var facultad = Nuevo(503);
        facultad.Facultad = 77;
        await Verificador.EsperarExcepcion<ReferenciaInvalidaExcepcion>(
            () => servicio.Crear(facultad),
            "programa completo: una facultad retirada es 400.");

        var area = Nuevo(504);
        area.AreasConocimiento.Add(new() { AreaConocimiento = "ZZZ" });
        await Verificador.EsperarExcepcion<ReferenciaInvalidaExcepcion>(
            () => servicio.Crear(area),
            "programa completo: un área retirada en el detalle es 400.");

        var practica = Nuevo(505);
        practica.PracticasEstrategias.Add(new() { PracticaEstrategia = 99 });
        await Verificador.EsperarExcepcion<ReferenciaInvalidaExcepcion>(
            () => servicio.Crear(practica),
            "programa completo: una práctica retirada es 400.");

        var caracteristica = Nuevo(506);
        caracteristica.CaracteristicasInnovacion.Add(new() { CarInnovacion = 99 });
        await Verificador.EsperarExcepcion<ReferenciaInvalidaExcepcion>(
            () => servicio.Crear(caracteristica),
            "programa completo: una característica retirada es 400.");

        var repetida = Nuevo(507);
        repetida.AreasConocimiento.Add(new() { AreaConocimiento = "1A01" });
        await Verificador.EsperarExcepcion<ArgumentException>(
            () => servicio.Crear(repetida),
            "programa completo: un área repetida en la lista es 400.");

        Verificador.Revisar(repositorio.Guardados.Count == 1,
            "programa completo: ningún intento fallido llegó al repositorio.",
            $"programa completo: el repositorio debía tener 1 programa y tiene {repositorio.Guardados.Count}.");
    }
}

/// <summary>El maestro-detalle de registro_calificado, en memoria.</summary>
public class RepositorioRegistroCalificadoCompletoFalso : IRepositorioRegistroCalificadoCompleto
{
    public Dictionary<int, RegistroCalificadoCompleto> Guardados { get; } = new();

    public Task Crear(RegistroCalificadoCompleto entidad)
    {
        if (!Guardados.TryAdd(entidad.Codigo, entidad))
        {
            throw new LlaveDuplicadaExcepcion("Ya existe un registro de registro_calificado con esa llave.");
        }

        return Task.CompletedTask;
    }

    public Task<RegistroCalificadoCompleto?> ObtenerCompleto(int codigo) =>
        Task.FromResult(Guardados.GetValueOrDefault(codigo));
}

/// <summary>El maestro-detalle de programa, en memoria.</summary>
public class RepositorioProgramaCompletoFalso : IRepositorioProgramaCompleto
{
    public Dictionary<int, ProgramaCompleto> Guardados { get; } = new();

    public Task Crear(ProgramaCompleto entidad)
    {
        if (!Guardados.TryAdd(entidad.Id, entidad))
        {
            throw new LlaveDuplicadaExcepcion("Ya existe un registro de programa con esa llave.");
        }

        return Task.CompletedTask;
    }

    public Task<ProgramaCompleto?> ObtenerCompleto(int id) =>
        Task.FromResult(Guardados.GetValueOrDefault(id));
}
