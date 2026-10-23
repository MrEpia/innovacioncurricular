using ApiInnovacion.Repositorios;

namespace ApiInnovacion.Pruebas;

/// <summary>
/// La integridad referencial, de mentiras: conjuntos en memoria con las llaves
/// de los padres «activos» y un diccionario con los hijos que cada padre tiene.
///
/// Cada prueba arma el escenario que necesita —«existe la facultad 10»,
/// «el programa 5 tiene 2 acreditaciones»— y el servicio real no nota que no
/// hay SQL Server detrás.
/// </summary>
public class RepositorioReferenciasFalso : IRepositorioReferencias
{
    public HashSet<int> Universidades { get; } = new();
    public HashSet<int> Facultades { get; } = new();
    public HashSet<int> Programas { get; } = new();
    public HashSet<int> RegistrosCalificados { get; } = new();
    public HashSet<int> ActividadesAcademicas { get; } = new();
    public HashSet<string> AreasConocimiento { get; } = new();
    public HashSet<int> PracticasEstrategias { get; } = new();
    public HashSet<int> CaracteristicasInnovacion { get; } = new();
    public HashSet<int> AspectosNormativos { get; } = new();
    public HashSet<int> Enfoques { get; } = new();
    public HashSet<int> Aliados { get; } = new();

    /// <summary>
    /// Los hijos activos de cada padre. La llave es "tabla:llave", por ejemplo
    /// "programa:5" → ["acreditacion (2)"]. Sin entrada = sin hijos.
    /// </summary>
    public Dictionary<string, List<string>> Dependientes { get; } = new();

    private IReadOnlyList<string> HijosDe(string tabla, object llave) =>
        Dependientes.TryGetValue($"{tabla}:{llave}", out var hijos) ? hijos : new List<string>();

    public Task<bool> UniversidadActiva(int id) => Task.FromResult(Universidades.Contains(id));
    public Task<bool> FacultadActiva(int id) => Task.FromResult(Facultades.Contains(id));
    public Task<bool> ProgramaActivo(int id) => Task.FromResult(Programas.Contains(id));
    public Task<bool> RegistroCalificadoActivo(int codigo) => Task.FromResult(RegistrosCalificados.Contains(codigo));
    public Task<bool> ActivAcademicaActiva(int id) => Task.FromResult(ActividadesAcademicas.Contains(id));
    public Task<bool> AreaConocimientoActiva(string id) => Task.FromResult(AreasConocimiento.Contains(id));
    public Task<bool> PracticaEstrategiaActiva(int id) => Task.FromResult(PracticasEstrategias.Contains(id));
    public Task<bool> CarInnovacionActiva(int id) => Task.FromResult(CaracteristicasInnovacion.Contains(id));
    public Task<bool> AspectoNormativoActivo(int id) => Task.FromResult(AspectosNormativos.Contains(id));
    public Task<bool> EnfoqueActivo(int id) => Task.FromResult(Enfoques.Contains(id));
    public Task<bool> AliadoActivo(int nit) => Task.FromResult(Aliados.Contains(nit));

    public Task<IReadOnlyList<string>> DependientesDeUniversidad(int id) => Task.FromResult(HijosDe("universidad", id));
    public Task<IReadOnlyList<string>> DependientesDeFacultad(int id) => Task.FromResult(HijosDe("facultad", id));
    public Task<IReadOnlyList<string>> DependientesDePrograma(int id) => Task.FromResult(HijosDe("programa", id));
    public Task<IReadOnlyList<string>> DependientesDeRegistroCalificado(int codigo) => Task.FromResult(HijosDe("registro_calificado", codigo));
    public Task<IReadOnlyList<string>> DependientesDeActivAcademica(int id) => Task.FromResult(HijosDe("activ_academica", id));
    public Task<IReadOnlyList<string>> DependientesDeAreaConocimiento(string id) => Task.FromResult(HijosDe("area_conocimiento", id));
    public Task<IReadOnlyList<string>> DependientesDePracticaEstrategia(int id) => Task.FromResult(HijosDe("practica_estrategia", id));
    public Task<IReadOnlyList<string>> DependientesDeCarInnovacion(int id) => Task.FromResult(HijosDe("car_innovacion", id));
    public Task<IReadOnlyList<string>> DependientesDeAspectoNormativo(int id) => Task.FromResult(HijosDe("aspecto_normativo", id));
    public Task<IReadOnlyList<string>> DependientesDeEnfoque(int id) => Task.FromResult(HijosDe("enfoque", id));
    public Task<IReadOnlyList<string>> DependientesDeAliado(int nit) => Task.FromResult(HijosDe("aliado", nit));
}
