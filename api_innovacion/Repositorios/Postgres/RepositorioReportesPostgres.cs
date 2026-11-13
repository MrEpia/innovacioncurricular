// PostgreSQL: consultas explícitas; conserva el contrato del repositorio SQL Server.
using System.Data;
using ApiInnovacion.Modelos;
using Dapper;
using Npgsql;

namespace ApiInnovacion.Repositorios.Postgres;

public class RepositorioReportesPostgres : IRepositorioReportes
{
    private readonly string _cadenaConexion;

    public RepositorioReportesPostgres(ConfiguracionPostgres configuracion)
    {
        _cadenaConexion = configuracion.CadenaConexion;
    }

    private async Task<IEnumerable<T>> Consultar<T>(string sql)
    {
        using IDbConnection conexion = new NpgsqlConnection(_cadenaConexion);
        return await conexion.QueryAsync<T>(sql);
    }

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

    public Task<IEnumerable<RegistroConEnfoques>> RegistrosConEnfoques() => Consultar<RegistroConEnfoques>(@"
        SELECT f.nombre AS Facultad, p.nombre AS Programa, r.codigo AS Registro, r.metodologia AS Metodologia,
               r.fecha_fin::timestamp AS VigenteHasta, COUNT(e.id) AS CantidadEnfoques,
               STRING_AGG(e.nombre, ', ' ORDER BY e.nombre) AS Enfoques
        FROM registro_calificado r
        JOIN programa p    ON p.id = r.programa  AND p.activo = 1
        JOIN facultad f    ON f.id = p.facultad  AND f.activo = 1
        JOIN enfoque_rc er ON er.registro_calificado = r.codigo AND er.activo = 1
        JOIN enfoque e     ON e.id = er.enfoque  AND e.activo = 1
        WHERE r.activo = 1
        GROUP BY f.nombre, p.nombre, r.codigo, r.metodologia, r.fecha_fin
        ORDER BY f.nombre, p.nombre");

    public Task<IEnumerable<ActividadDelPlan>> PlanDeEstudios() => Consultar<ActividadDelPlan>(@"
        SELECT p.nombre AS Programa, r.codigo AS Registro, a.nombre AS Actividad, a.num_creditos AS Creditos,
               x.componente AS Componente, x.semestre AS Semestre, a.idioma AS Idioma
        FROM aa_rc x
        JOIN registro_calificado r ON r.codigo = x.registro_calificado_codigo AND r.activo = 1
        JOIN programa p            ON p.id = r.programa                       AND p.activo = 1
        JOIN activ_academica a     ON a.id = x.activ_academicas_idcurso       AND a.activo = 1
        WHERE x.activo = 1
        ORDER BY p.nombre, CASE WHEN btrim(x.semestre) = '' THEN 0 WHEN btrim(x.semestre) ~ '^[+-]?[0-9]{1,10}$' THEN CASE WHEN btrim(x.semestre)::bigint BETWEEN -2147483648 AND 2147483647 THEN btrim(x.semestre)::integer END END, a.nombre");

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

    public Task<IEnumerable<InnovacionDePrograma>> InnovacionPorPrograma() => Consultar<InnovacionDePrograma>(@"
        SELECT f.nombre AS Facultad, p.nombre AS Programa,
               COALESCE((SELECT STRING_AGG(c.nombre, ', ' ORDER BY c.nombre)
                       FROM programa_ci pc JOIN car_innovacion c ON c.id = pc.car_innovacion AND c.activo = 1
                       WHERE pc.programa = p.id AND pc.activo = 1), '') AS Caracteristicas,
               COALESCE((SELECT STRING_AGG(e.nombre, ', ' ORDER BY e.nombre)
                       FROM programa_pe pp JOIN practica_estrategia e ON e.id = pp.practica_estrategia AND e.activo = 1
                       WHERE pp.programa = p.id AND pp.activo = 1), '') AS Practicas
        FROM programa p
        JOIN facultad f ON f.id = p.facultad AND f.activo = 1
        WHERE p.activo = 1
          AND (EXISTS (SELECT 1 FROM programa_ci pc WHERE pc.programa = p.id AND pc.activo = 1)
               OR EXISTS (SELECT 1 FROM programa_pe pp WHERE pp.programa = p.id AND pp.activo = 1))
        ORDER BY f.nombre, p.nombre");

    public Task<IEnumerable<AlianzaVigente>> AlianzasVigentes() => Consultar<AlianzaVigente>(@"
        SELECT al.razon_social AS Aliado, al.ciudad AS CiudadAliado, p.nombre AS Programa, f.nombre AS Facultad,
               u.ciudad AS Sede, z.fecha_inicio::timestamp AS Desde, z.fecha_fin::timestamp AS Hasta, z.docente AS Docente
        FROM alianza z
        JOIN aliado al     ON al.nit = z.aliado       AND al.activo = 1
        JOIN programa p    ON p.id = z.departamento   AND p.activo = 1
        JOIN facultad f    ON f.id = p.facultad       AND f.activo = 1
        JOIN universidad u ON u.id = f.universidad    AND u.activo = 1
        WHERE z.activo = 1 AND (z.fecha_fin IS NULL OR z.fecha_fin >= CAST(LOCALTIMESTAMP AS DATE))
        ORDER BY al.razon_social, p.nombre");

    public Task<IEnumerable<NormaDePrograma>> NormatividadPorPrograma() => Consultar<NormaDePrograma>(@"
        SELECT n.descripcion AS Norma, n.tipo AS Tipo, n.fuente AS Fuente, p.nombre AS Programa, f.nombre AS Facultad
        FROM an_programa ap
        JOIN aspecto_normativo n ON n.id = ap.aspecto_normativo AND n.activo = 1
        JOIN programa p          ON p.id = ap.programa          AND p.activo = 1
        JOIN facultad f          ON f.id = p.facultad           AND f.activo = 1
        WHERE ap.activo = 1
        ORDER BY n.descripcion, p.nombre");

    public Task<IEnumerable<InternacionalizacionDePrograma>> Internacionalizacion() => Consultar<InternacionalizacionDePrograma>(@"
        SELECT f.nombre AS Facultad, p.nombre AS Programa,
               (SELECT COUNT(*) FROM pasantia s WHERE s.programa = p.id AND s.activo = 1) AS Pasantias,
               COALESCE((SELECT STRING_AGG(x.pais, ', ' ORDER BY x.pais)
                       FROM (SELECT DISTINCT s.pais FROM pasantia s WHERE s.programa = p.id AND s.activo = 1) x), '') AS PaisesPasantia,
               (SELECT COUNT(*) FROM premio m WHERE m.programa = p.id AND m.activo = 1) AS Premios,
               COALESCE((SELECT m.nombre FROM premio m WHERE m.programa = p.id AND m.activo = 1
                       ORDER BY m.fecha DESC LIMIT 1), '') AS UltimoPremio
        FROM programa p
        JOIN facultad f ON f.id = p.facultad AND f.activo = 1
        WHERE p.activo = 1
          AND (EXISTS (SELECT 1 FROM pasantia s WHERE s.programa = p.id AND s.activo = 1)
               OR EXISTS (SELECT 1 FROM premio m WHERE m.programa = p.id AND m.activo = 1))
        ORDER BY f.nombre, p.nombre");

    public Task<IEnumerable<DocentesDeSede>> DocentesPorSede() => Consultar<DocentesDeSede>(@"
        SELECT u.ciudad AS Sede, f.nombre AS Facultad, d.dedicacion AS Dedicacion, COUNT(*) AS Docentes
        FROM docente_departamento d
        JOIN programa p    ON p.id = d.departamento AND p.activo = 1
        JOIN facultad f    ON f.id = p.facultad     AND f.activo = 1
        JOIN universidad u ON u.id = f.universidad  AND u.activo = 1
        WHERE d.activo = 1 AND (d.fecha_salida IS NULL OR d.fecha_salida >= CAST(LOCALTIMESTAMP AS DATE))
        GROUP BY u.ciudad, f.nombre, d.dedicacion
        ORDER BY u.ciudad, f.nombre, d.dedicacion");

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
