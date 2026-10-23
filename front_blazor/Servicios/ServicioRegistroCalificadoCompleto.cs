namespace FrontInnovacion.Servicios;

/// <summary>
/// registro_calificado con su detalle, tal como el front lo maneja: el mismo
/// registro de siempre más las dos listas de hijos.
/// </summary>
public class RegistroCalificadoCompleto : RegistroCalificado
{
    public List<EnfoqueDelRegistro> Enfoques { get; set; } = new();

    public List<ActividadDelRegistro> Actividades { get; set; } = new();
}

/// <summary>Una fila de la tabla editable de enfoques.</summary>
public class EnfoqueDelRegistro
{
    public int? Enfoque { get; set; }

    /// <summary>Solo lo llena la API al leer.</summary>
    public string? Nombre { get; set; }
}

/// <summary>Una fila de la tabla editable de actividades.</summary>
public class ActividadDelRegistro
{
    public int? ActivAcademicasIdcurso { get; set; }

    public string Componente { get; set; } = string.Empty;

    public string Semestre { get; set; } = string.Empty;

    /// <summary>Solo lo llena la API al leer.</summary>
    public string? Nombre { get; set; }
}

/// <summary>
/// La capa de datos del front para el maestro-detalle de registro_calificado.
/// Sabe sus dos rutas; el transporte lo pone ClienteApi.
/// </summary>
public class ServicioRegistroCalificadoCompleto
{
    private readonly ClienteApi _api;

    public ServicioRegistroCalificadoCompleto(ClienteApi api)
    {
        _api = api;
    }

    // MD2 — El registro con su detalle.
    public Task<Resultado<RegistroCalificadoCompleto>> Obtener(int codigo) =>
        _api.Obtener<RegistroCalificadoCompleto>($"/api/registro_calificado/{codigo}/completo");

    // MD1 — Guardar el registro con TODO su detalle en una petición: o se
    // guarda todo o nada. Los hijos viajan solo con sus datos (el nombre es
    // de lectura); una fila sin enfoque elegido viaja vacía y la API dirá 422.
    public Task<Resultado<bool>> Crear(RegistroCalificadoCompleto entidad) =>
        _api.Enviar(HttpMethod.Post, "/api/registro_calificado/completo", new
        {
            codigo = entidad.Codigo,
            cantCreditos = entidad.CantCreditos,
            horaAcom = entidad.HoraAcom,
            horaInd = entidad.HoraInd,
            metodologia = entidad.Metodologia,
            fechaInicio = entidad.FechaInicio,
            fechaFin = entidad.FechaFin,
            duracionAnios = entidad.DuracionAnios,
            duracionSemestres = entidad.DuracionSemestres,
            tipoTitulacion = entidad.TipoTitulacion,
            programa = entidad.Programa,
            enfoques = entidad.Enfoques.Select(e => e.Enfoque),
            actividades = entidad.Actividades.Select(a => new
            {
                activAcademicasIdcurso = a.ActivAcademicasIdcurso,
                componente = a.Componente,
                semestre = a.Semestre
            })
        });
}
