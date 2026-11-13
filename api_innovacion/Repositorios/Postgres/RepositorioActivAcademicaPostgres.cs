// PostgreSQL: consultas explícitas; conserva el contrato del repositorio SQL Server.
using System.Data;
using ApiInnovacion.Excepciones;
using ApiInnovacion.Modelos;
using Dapper;
using Npgsql;

namespace ApiInnovacion.Repositorios.Postgres;

public class RepositorioActivAcademicaPostgres : IRepositorioActivAcademica
{
    private readonly string _cadenaConexion;

    public RepositorioActivAcademicaPostgres(ConfiguracionPostgres configuracion)
    {
        _cadenaConexion = configuracion.CadenaConexion;
    }

    private IDbConnection CrearConexion() => new NpgsqlConnection(_cadenaConexion);

    private const string COLUMNAS = @"id AS Id, nombre AS Nombre, num_creditos AS NumCreditos, tipo AS Tipo, area_formacion AS AreaFormacion, h_acom AS HAcom, h_indep AS HIndep, idioma AS Idioma, espejo AS Espejo, entidad_espejo AS EntidadEspejo, pais_espejo AS PaisEspejo, disenio AS Disenio";

    public async Task<IEnumerable<ActivAcademica>> ObtenerTodos(int limite)
    {
        using var conexion = CrearConexion();
        var sql = $@"
            SELECT {COLUMNAS}
            FROM activ_academica
            WHERE activo = 1
            ORDER BY id ASC LIMIT @Limite";

        return await conexion.QueryAsync<ActivAcademica>(sql, new { Limite = limite });
    }

    public async Task<ActivAcademica?> ObtenerPorId(int id)
    {
        using var conexion = CrearConexion();

        var sql = $@"
            SELECT {COLUMNAS}
            FROM activ_academica
            WHERE id = @Id AND activo = 1";

        return await conexion.QueryFirstOrDefaultAsync<ActivAcademica>(sql, new { Id = id });
    }

    public async Task Crear(ActivAcademica entidad)
    {
        using var conexion = CrearConexion();
        const string sql = @"
            INSERT INTO activ_academica (id, nombre, num_creditos, tipo, area_formacion, h_acom, h_indep, idioma, espejo, entidad_espejo, pais_espejo, disenio, activo)
            VALUES (@Id, @Nombre, @NumCreditos, @Tipo, @AreaFormacion, @HAcom, @HIndep, @Idioma, @Espejo, @EntidadEspejo, @PaisEspejo, @Disenio, 1)";

        try
        {
            await conexion.ExecuteAsync(sql, entidad);
        }
        catch (PostgresException ex) when (ex.SqlState == "23505")
        {

            throw new LlaveDuplicadaExcepcion(
                "Ya existe un registro de activ_academica con esa llave (puede estar retirado).");
        }
    }

    public async Task<int> Reemplazar(ActivAcademica entidad)
    {
        using var conexion = CrearConexion();
        const string sql = @"
            UPDATE activ_academica
            SET nombre = @Nombre, num_creditos = @NumCreditos, tipo = @Tipo, area_formacion = @AreaFormacion, h_acom = @HAcom, h_indep = @HIndep, idioma = @Idioma, espejo = @Espejo, entidad_espejo = @EntidadEspejo, pais_espejo = @PaisEspejo, disenio = @Disenio
            WHERE id = @Id AND activo = 1";

        return await conexion.ExecuteAsync(sql, entidad);
    }

    public async Task<int> ActualizarParcial(int id, ActivAcademicaCambios cambios)
    {
        using var conexion = CrearConexion();

        var asignaciones = new List<string>();
        var parametros = new DynamicParameters();
        parametros.Add("Id", id);

        if (cambios.Nombre != null)
        {
            asignaciones.Add("nombre = @Nombre");
            parametros.Add("Nombre", cambios.Nombre);
        }
        if (cambios.NumCreditos != null)
        {
            asignaciones.Add("num_creditos = @NumCreditos");
            parametros.Add("NumCreditos", cambios.NumCreditos);
        }
        if (cambios.Tipo != null)
        {
            asignaciones.Add("tipo = @Tipo");
            parametros.Add("Tipo", cambios.Tipo);
        }
        if (cambios.AreaFormacion != null)
        {
            asignaciones.Add("area_formacion = @AreaFormacion");
            parametros.Add("AreaFormacion", cambios.AreaFormacion);
        }
        if (cambios.HAcom != null)
        {
            asignaciones.Add("h_acom = @HAcom");
            parametros.Add("HAcom", cambios.HAcom);
        }
        if (cambios.HIndep != null)
        {
            asignaciones.Add("h_indep = @HIndep");
            parametros.Add("HIndep", cambios.HIndep);
        }
        if (cambios.Idioma != null)
        {
            asignaciones.Add("idioma = @Idioma");
            parametros.Add("Idioma", cambios.Idioma);
        }
        if (cambios.Espejo != null)
        {
            asignaciones.Add("espejo = @Espejo");
            parametros.Add("Espejo", cambios.Espejo);
        }
        if (cambios.EntidadEspejo != null)
        {
            asignaciones.Add("entidad_espejo = @EntidadEspejo");
            parametros.Add("EntidadEspejo", cambios.EntidadEspejo);
        }
        if (cambios.PaisEspejo != null)
        {
            asignaciones.Add("pais_espejo = @PaisEspejo");
            parametros.Add("PaisEspejo", cambios.PaisEspejo);
        }
        if (cambios.Disenio != null)
        {
            asignaciones.Add("disenio = @Disenio");
            parametros.Add("Disenio", cambios.Disenio);
        }

        if (asignaciones.Count == 0) return 0;

        var sql = $"UPDATE activ_academica SET {string.Join(", ", asignaciones)} WHERE id = @Id AND activo = 1";

        return await conexion.ExecuteAsync(sql, parametros);
    }

    public async Task<int> EliminarLogico(int id)
    {
        using var conexion = CrearConexion();

        try
        {
            return await conexion.ExecuteScalarAsync<int>(
                "SELECT * FROM public.sp_retirar_activ_academica(@Id)", new { Id = id });
        }
        catch (PostgresException ex) when (ex.SqlState == "P0409")
        {
            throw new DependenciasActivasExcepcion(
                $"No se puede retirar la actividad académica {id}: {ex.MessageText}. Retírelos primero.");
        }
    }
}
