namespace ApiInnovacion.Repositorios;

/// <summary>
/// Las preguntas de INTEGRIDAD REFERENCIAL que hacen los servicios de la v2:
///
///   1. ¿Existe y está ACTIVO el registro al que apunta esta clave foránea?
///      Se pregunta antes de crear o modificar una fila hija.
///   2. ¿Qué registros ACTIVOS dependen de este? Se pregunta antes de retirar
///      una fila padre.
///
/// Un método explícito por pregunta, con su SQL a la vista, en vez de un
/// «¿existe la fila X en la tabla Y?» genérico: la tabla y la columna no
/// viajan nunca como texto (Artículo 10.1), y el compilador sabe qué
/// preguntas existen.
///
/// Es una interfaz por la misma razón que los demás repositorios: la prueba de
/// capas le enchufa una versión en memoria y los servicios no se enteran.
/// </summary>
public interface IRepositorioReferencias
{
    // --- 1. ¿Existe y está activo el padre? -------------------------------
    Task<bool> UniversidadActiva(int id);
    Task<bool> FacultadActiva(int id);
    Task<bool> ProgramaActivo(int id);
    Task<bool> RegistroCalificadoActivo(int codigo);
    Task<bool> ActivAcademicaActiva(int id);
    Task<bool> AreaConocimientoActiva(string id);
    Task<bool> PracticaEstrategiaActiva(int id);
    Task<bool> CarInnovacionActiva(int id);
    Task<bool> AspectoNormativoActivo(int id);
    Task<bool> EnfoqueActivo(int id);
    Task<bool> AliadoActivo(int nit);

    // v3: los extremos de una asignación rol_usuario.
    Task<bool> UsuarioActivo(int id);
    Task<bool> RolActivo(int id);

    // --- 2. ¿Quién depende de él? -----------------------------------------
    // Cada uno devuelve las tablas con hijos activos y cuántos, por ejemplo
    // ["programa (3)"]. Lista vacía = se puede retirar sin dejar huérfanos.
    Task<IReadOnlyList<string>> DependientesDeUniversidad(int id);
    Task<IReadOnlyList<string>> DependientesDeFacultad(int id);
    Task<IReadOnlyList<string>> DependientesDePrograma(int id);
    Task<IReadOnlyList<string>> DependientesDeRegistroCalificado(int codigo);
    Task<IReadOnlyList<string>> DependientesDeActivAcademica(int id);
    Task<IReadOnlyList<string>> DependientesDeAreaConocimiento(string id);
    Task<IReadOnlyList<string>> DependientesDePracticaEstrategia(int id);
    Task<IReadOnlyList<string>> DependientesDeCarInnovacion(int id);
    Task<IReadOnlyList<string>> DependientesDeAspectoNormativo(int id);
    Task<IReadOnlyList<string>> DependientesDeEnfoque(int id);
    Task<IReadOnlyList<string>> DependientesDeAliado(int nit);
}
