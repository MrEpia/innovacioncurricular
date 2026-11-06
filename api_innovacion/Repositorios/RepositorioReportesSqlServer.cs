using System.Data;
using ApiInnovacion.Modelos;
using Dapper;
using Microsoft.Data.SqlClient;

namespace ApiInnovacion.Repositorios;

/// <summary>
/// v4 — Las 10 consultas multitabla contra SQL Server. Cada una une AL MENOS
/// CUATRO tablas, está escrita completa (Artículo 2: SQL a la vista) y filtra
/// activo = 1 en TODAS las tablas que toca: un padre retirado no aparece, ni
/// tampoco los hijos que todavía lo apuntan.
///
/// No reciben parámetros del usuario, así que no hay nada que parametrizar; el
/// día que tengan filtros, irán como @parametro.
/// </summary>
public class RepositorioReportesSqlServer : IRepositorioReportes
{
    private readonly string _cadenaConexion;

    public RepositorioReportesSqlServer(IConfiguration configuracion)
    {
        _cadenaConexion = configuracion.GetConnectionString("SqlServer")
            ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'SqlServer'.");
    }

    private async Task<IEnumerable<T>> Consultar<T>(string sql)
    {
        using IDbConnection conexion = new SqlConnection(_cadenaConexion);
        return await conexion.QueryAsync<T>(sql);
    }

    /// <summary>R1 — 4 tablas: acreditacion · programa · facultad · universidad.</summary>
    public Task<IEnumerable<ProgramaAcreditado>> ProgramasAcreditados() => Consultar<ProgramaAcreditado>(@"
        SELECT u.nombre AS Universidad, u.ciudad AS Ciudad, f.nombre AS Facultad, p.nombre AS Programa,
               p.nivel AS Nivel, a.resolucion AS Resolucion, a.tipo AS TipoAcreditacion,
               a.calificacion AS Calificacion, a.fecha_inicio AS Desde, a.fecha_fin AS Hasta
        FROM acreditacion a
        JOIN programa p    ON p.id = a.programa    AND p.activo = 1
        JOIN facultad f    ON f.id = p.facultad    AND f.activo = 1
        JOIN universidad u ON u.id = f.universidad AND u.activo = 1
        WHERE a.activo = 1
        ORDER BY u.ciudad, f.nombre, p.nombre");

    /// <summary>R2 — 5 tablas: facultad · programa · registro_calificado · enfoque_rc · enfoque.</summary>
    public Task<IEnumerable<RegistroConEnfoques>> RegistrosConEnfoques() => Consultar<RegistroConEnfoques>(@"
        SELECT f.nombre AS Facultad, p.nombre AS Programa, r.codigo AS Registro, r.metodologia AS Metodologia,
               r.fecha_fin AS VigenteHasta, COUNT(e.id) AS CantidadEnfoques,
               STRING_AGG(e.nombre, ', ') WITHIN GROUP (ORDER BY e.nombre) AS Enfoques
        FROM registro_calificado r
        JOIN programa p    ON p.id = r.programa  AND p.activo = 1
        JOIN facultad f    ON f.id = p.facultad  AND f.activo = 1
        JOIN enfoque_rc er ON er.registro_calificado = r.codigo AND er.activo = 1
        JOIN enfoque e     ON e.id = er.enfoque  AND e.activo = 1
        WHERE r.activo = 1
        GROUP BY f.nombre, p.nombre, r.codigo, r.metodologia, r.fecha_fin
        ORDER BY f.nombre, p.nombre");

    /// <summary>R3 — 4 tablas: programa · registro_calificado · aa_rc · activ_academica.</summary>
    public Task<IEnumerable<ActividadDelPlan>> PlanDeEstudios() => Consultar<ActividadDelPlan>(@"
        SELECT p.nombre AS Programa, r.codigo AS Registro, a.nombre AS Actividad, a.num_creditos AS Creditos,
               x.componente AS Componente, x.semestre AS Semestre, a.idioma AS Idioma
        FROM aa_rc x
        JOIN registro_calificado r ON r.codigo = x.registro_calificado_codigo AND r.activo = 1
        JOIN programa p            ON p.id = r.programa                       AND p.activo = 1
        JOIN activ_academica a     ON a.id = x.activ_academicas_idcurso       AND a.activo = 1
        WHERE x.activo = 1
        ORDER BY p.nombre, TRY_CAST(x.semestre AS INT), a.nombre");

    /// <summary>R4 — 4 tablas: facultad · programa · programa_ac · area_conocimiento.</summary>
    public Task<IEnumerable<AreasDeFacultad>> AreasPorFacultad() => Consultar<AreasDeFacultad>(@"
        SELECT f.nombre AS Facultad, ac.gran_area AS GranArea,
               COUNT(DISTINCT p.id) AS Programas, COUNT(DISTINCT ac.id) AS Areas
        FROM facultad f
        JOIN programa p           ON p.facultad = f.id          AND p.activo = 1
        JOIN programa_ac pa       ON pa.programa = p.id         AND pa.activo = 1
        JOIN area_conocimiento ac ON ac.id = pa.area_conocimiento AND ac.activo = 1
        WHERE f.activo = 1
        GROUP BY f.nombre, ac.gran_area
        ORDER BY f.nombre, ac.gran_area");

    /// <summary>R5 — 6 tablas: programa · facultad · programa_ci · car_innovacion · programa_pe · practica_estrategia.</summary>
    public Task<IEnumerable<InnovacionDePrograma>> InnovacionPorPrograma() => Consultar<InnovacionDePrograma>(@"
        SELECT f.nombre AS Facultad, p.nombre AS Programa,
               ISNULL((SELECT STRING_AGG(c.nombre, ', ') WITHIN GROUP (ORDER BY c.nombre)
                       FROM programa_ci pc JOIN car_innovacion c ON c.id = pc.car_innovacion AND c.activo = 1
                       WHERE pc.programa = p.id AND pc.activo = 1), '') AS Caracteristicas,
               ISNULL((SELECT STRING_AGG(e.nombre, ', ') WITHIN GROUP (ORDER BY e.nombre)
                       FROM programa_pe pp JOIN practica_estrategia e ON e.id = pp.practica_estrategia AND e.activo = 1
                       WHERE pp.programa = p.id AND pp.activo = 1), '') AS Practicas
        FROM programa p
        JOIN facultad f ON f.id = p.facultad AND f.activo = 1
        WHERE p.activo = 1
          AND (EXISTS (SELECT 1 FROM programa_ci pc WHERE pc.programa = p.id AND pc.activo = 1)
               OR EXISTS (SELECT 1 FROM programa_pe pp WHERE pp.programa = p.id AND pp.activo = 1))
        ORDER BY f.nombre, p.nombre");

    /// <summary>R6 — 5 tablas: aliado · alianza · programa · facultad · universidad. Vigente = sin fin o fin futuro.</summary>
    public Task<IEnumerable<AlianzaVigente>> AlianzasVigentes() => Consultar<AlianzaVigente>(@"
        SELECT al.razon_social AS Aliado, al.ciudad AS CiudadAliado, p.nombre AS Programa, f.nombre AS Facultad,
               u.ciudad AS Sede, z.fecha_inicio AS Desde, z.fecha_fin AS Hasta, z.docente AS Docente
        FROM alianza z
        JOIN aliado al     ON al.nit = z.aliado       AND al.activo = 1
        JOIN programa p    ON p.id = z.departamento   AND p.activo = 1
        JOIN facultad f    ON f.id = p.facultad       AND f.activo = 1
        JOIN universidad u ON u.id = f.universidad    AND u.activo = 1
        WHERE z.activo = 1 AND (z.fecha_fin IS NULL OR z.fecha_fin >= CAST(GETDATE() AS DATE))
        ORDER BY al.razon_social, p.nombre");

    /// <summary>R7 — 4 tablas: aspecto_normativo · an_programa · programa · facultad.</summary>
    public Task<IEnumerable<NormaDePrograma>> NormatividadPorPrograma() => Consultar<NormaDePrograma>(@"
        SELECT n.descripcion AS Norma, n.tipo AS Tipo, n.fuente AS Fuente, p.nombre AS Programa, f.nombre AS Facultad
        FROM an_programa ap
        JOIN aspecto_normativo n ON n.id = ap.aspecto_normativo AND n.activo = 1
        JOIN programa p          ON p.id = ap.programa          AND p.activo = 1
        JOIN facultad f          ON f.id = p.facultad           AND f.activo = 1
        WHERE ap.activo = 1
        ORDER BY n.descripcion, p.nombre");

    /// <summary>R8 — 4 tablas: programa · facultad · pasantia · premio.</summary>
    public Task<IEnumerable<InternacionalizacionDePrograma>> Internacionalizacion() => Consultar<InternacionalizacionDePrograma>(@"
        SELECT f.nombre AS Facultad, p.nombre AS Programa,
               (SELECT COUNT(*) FROM pasantia s WHERE s.programa = p.id AND s.activo = 1) AS Pasantias,
               ISNULL((SELECT STRING_AGG(x.pais, ', ') WITHIN GROUP (ORDER BY x.pais)
                       FROM (SELECT DISTINCT s.pais FROM pasantia s WHERE s.programa = p.id AND s.activo = 1) x), '') AS PaisesPasantia,
               (SELECT COUNT(*) FROM premio m WHERE m.programa = p.id AND m.activo = 1) AS Premios,
               ISNULL((SELECT TOP 1 m.nombre FROM premio m WHERE m.programa = p.id AND m.activo = 1
                       ORDER BY m.fecha DESC), '') AS UltimoPremio
        FROM programa p
        JOIN facultad f ON f.id = p.facultad AND f.activo = 1
        WHERE p.activo = 1
          AND (EXISTS (SELECT 1 FROM pasantia s WHERE s.programa = p.id AND s.activo = 1)
               OR EXISTS (SELECT 1 FROM premio m WHERE m.programa = p.id AND m.activo = 1))
        ORDER BY f.nombre, p.nombre");

    /// <summary>R9 — 4 tablas: universidad · facultad · programa · docente_departamento.</summary>
    public Task<IEnumerable<DocentesDeSede>> DocentesPorSede() => Consultar<DocentesDeSede>(@"
        SELECT u.ciudad AS Sede, f.nombre AS Facultad, d.dedicacion AS Dedicacion, COUNT(*) AS Docentes
        FROM docente_departamento d
        JOIN programa p    ON p.id = d.departamento AND p.activo = 1
        JOIN facultad f    ON f.id = p.facultad     AND f.activo = 1
        JOIN universidad u ON u.id = f.universidad  AND u.activo = 1
        WHERE d.activo = 1 AND (d.fecha_salida IS NULL OR d.fecha_salida >= CAST(GETDATE() AS DATE))
        GROUP BY u.ciudad, f.nombre, d.dedicacion
        ORDER BY u.ciudad, f.nombre, d.dedicacion");

    /// <summary>R10 — 4 tablas: activ_academica · programa · facultad · universidad. Solo las que tienen espejo.</summary>
    public Task<IEnumerable<CursoEspejo>> CursosEspejo() => Consultar<CursoEspejo>(@"
        SELECT a.nombre AS Actividad, a.num_creditos AS Creditos, a.idioma AS Idioma,
               a.entidad_espejo AS EntidadEspejo, a.pais_espejo AS PaisEspejo,
               p.nombre AS Programa, f.nombre AS Facultad, u.ciudad AS Sede
        FROM activ_academica a
        JOIN programa p    ON p.id = a.disenio     AND p.activo = 1
        JOIN facultad f    ON f.id = p.facultad    AND f.activo = 1
        JOIN universidad u ON u.id = f.universidad AND u.activo = 1
        WHERE a.activo = 1 AND a.espejo = 1
        ORDER BY a.pais_espejo, a.nombre");
}
