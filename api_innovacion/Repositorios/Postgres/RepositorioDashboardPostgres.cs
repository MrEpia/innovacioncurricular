// PostgreSQL: consultas explícitas; conserva el contrato del repositorio SQL Server.
using System.Data;
using ApiInnovacion.Modelos;
using Dapper;
using Npgsql;

namespace ApiInnovacion.Repositorios.Postgres;

public class RepositorioDashboardPostgres : IRepositorioDashboard
{
    private readonly string _cadenaConexion;

    public RepositorioDashboardPostgres(ConfiguracionPostgres configuracion)
    {
        _cadenaConexion = configuracion.CadenaConexion;
    }

    private IDbConnection CrearConexion() => new NpgsqlConnection(_cadenaConexion);

    private async Task<IEnumerable<DatoGrafico>> Agrupar(string sql)
    {
        using var conexion = CrearConexion();
        return await conexion.QueryAsync<DatoGrafico>(sql);
    }

    public async Task<ResumenDashboard> Resumen()
    {
        using var conexion = CrearConexion();
        return await conexion.QuerySingleAsync<ResumenDashboard>(@"
            SELECT
                (SELECT COUNT(*) FROM facultad WHERE activo = 1)              AS Facultades,
                (SELECT COUNT(*) FROM programa WHERE activo = 1)              AS Programas,
                (SELECT COUNT(*) FROM acreditacion WHERE activo = 1)          AS Acreditaciones,
                (SELECT COUNT(*) FROM registro_calificado WHERE activo = 1)   AS RegistrosCalificados,
                (SELECT COUNT(*) FROM activ_academica WHERE activo = 1)       AS ActividadesAcademicas,
                (SELECT COUNT(DISTINCT docente) FROM docente_departamento WHERE activo = 1) AS Docentes,
                (SELECT COUNT(*) FROM alianza WHERE activo = 1)               AS Alianzas,
                (SELECT COUNT(*) FROM pasantia WHERE activo = 1)              AS Pasantias,
                (SELECT COUNT(*) FROM premio WHERE activo = 1)                AS Premios,
                (SELECT COALESCE(SUM(CASE WHEN btrim(cant_graduados) = '' THEN 0 WHEN btrim(cant_graduados) ~ '^[+-]?[0-9]{1,10}$' THEN CASE WHEN btrim(cant_graduados)::bigint BETWEEN -2147483648 AND 2147483647 THEN btrim(cant_graduados)::integer END END), 0) FROM programa WHERE activo = 1) AS Graduados");
    }

    public Task<IEnumerable<DatoGrafico>> ProgramasPorNivel() => Agrupar(@"
        SELECT nivel AS Etiqueta, COUNT(*) AS Valor
        FROM programa WHERE activo = 1
        GROUP BY nivel ORDER BY Valor DESC, nivel");

    public Task<IEnumerable<DatoGrafico>> ProgramasPorSede() => Agrupar(@"
        SELECT u.ciudad AS Etiqueta, COUNT(*) AS Valor
        FROM programa p
        JOIN facultad f    ON f.id = p.facultad    AND f.activo = 1
        JOIN universidad u ON u.id = f.universidad AND u.activo = 1
        WHERE p.activo = 1
        GROUP BY u.ciudad ORDER BY Valor DESC, u.ciudad");

    public Task<IEnumerable<DatoGrafico>> AcreditacionesPorCalificacion() => Agrupar(@"
        SELECT calificacion AS Etiqueta, COUNT(*) AS Valor
        FROM acreditacion WHERE activo = 1
        GROUP BY calificacion ORDER BY Valor DESC, calificacion");

    public Task<IEnumerable<DatoGrafico>> RegistrosPorMetodologia() => Agrupar(@"
        SELECT metodologia AS Etiqueta, COUNT(*) AS Valor
        FROM registro_calificado WHERE activo = 1
        GROUP BY metodologia ORDER BY Valor DESC, metodologia");

    public Task<IEnumerable<DatoGrafico>> DocentesPorDedicacion() => Agrupar(@"
        SELECT dedicacion AS Etiqueta, COUNT(*) AS Valor
        FROM docente_departamento WHERE activo = 1
        GROUP BY dedicacion ORDER BY Valor DESC, dedicacion");

    public Task<IEnumerable<DatoGrafico>> GraduadosPorFacultad() => Agrupar(@"
        SELECT f.nombre AS Etiqueta, COALESCE(SUM(CASE WHEN btrim(p.cant_graduados) = '' THEN 0 WHEN btrim(p.cant_graduados) ~ '^[+-]?[0-9]{1,10}$' THEN CASE WHEN btrim(p.cant_graduados)::bigint BETWEEN -2147483648 AND 2147483647 THEN btrim(p.cant_graduados)::integer END END), 0) AS Valor
        FROM facultad f
        JOIN programa p ON p.facultad = f.id AND p.activo = 1
        WHERE f.activo = 1
        GROUP BY f.nombre ORDER BY Valor DESC, f.nombre");
}
