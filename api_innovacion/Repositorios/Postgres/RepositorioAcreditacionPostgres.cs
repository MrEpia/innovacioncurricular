// PostgreSQL: consultas explícitas; conserva el contrato del repositorio SQL Server.
using System.Data;
using ApiInnovacion.Excepciones;
using ApiInnovacion.Modelos;
using Dapper;
using Npgsql;

namespace ApiInnovacion.Repositorios.Postgres;

public class RepositorioAcreditacionPostgres : IRepositorioAcreditacion
{
    private readonly string _cadenaConexion;

    public RepositorioAcreditacionPostgres(ConfiguracionPostgres configuracion)
    {
        _cadenaConexion = configuracion.CadenaConexion;
    }

    private IDbConnection CrearConexion() => new NpgsqlConnection(_cadenaConexion);

    private const string COLUMNAS = @"resolucion AS Resolucion, tipo AS Tipo, calificacion AS Calificacion, fecha_inicio AS FechaInicio, fecha_fin AS FechaFin, programa AS Programa";

    public async Task<IEnumerable<Acreditacion>> ObtenerTodos(int limite)
    {
        using var conexion = CrearConexion();
        var sql = $@"
            SELECT {COLUMNAS}
            FROM acreditacion
            WHERE activo = 1
            ORDER BY resolucion ASC LIMIT @Limite";

        return await conexion.QueryAsync<Acreditacion>(sql, new { Limite = limite });
    }

    public async Task<Acreditacion?> ObtenerPorResolucion(int resolucion)
    {
        using var conexion = CrearConexion();

        var sql = $@"
            SELECT {COLUMNAS}
            FROM acreditacion
            WHERE resolucion = @Resolucion AND activo = 1";

        return await conexion.QueryFirstOrDefaultAsync<Acreditacion>(sql, new { Resolucion = resolucion });
    }

    public async Task Crear(Acreditacion entidad)
    {
        using var conexion = CrearConexion();
        const string sql = @"
            INSERT INTO acreditacion (resolucion, tipo, calificacion, fecha_inicio, fecha_fin, programa, activo)
            VALUES (@Resolucion, @Tipo, @Calificacion, @FechaInicio, @FechaFin, @Programa, 1)";

        try
        {
            await conexion.ExecuteAsync(sql, entidad);
        }
        catch (PostgresException ex) when (ex.SqlState == "23505")
        {

            throw new LlaveDuplicadaExcepcion(
                "Ya existe un registro de acreditacion con esa llave (puede estar retirado).");
        }
    }

    public async Task<int> Reemplazar(Acreditacion entidad)
    {
        using var conexion = CrearConexion();
        const string sql = @"
            UPDATE acreditacion
            SET tipo = @Tipo, calificacion = @Calificacion, fecha_inicio = @FechaInicio, fecha_fin = @FechaFin, programa = @Programa
            WHERE resolucion = @Resolucion AND activo = 1";

        return await conexion.ExecuteAsync(sql, entidad);
    }

    public async Task<int> ActualizarParcial(int resolucion, AcreditacionCambios cambios)
    {
        using var conexion = CrearConexion();

        var asignaciones = new List<string>();
        var parametros = new DynamicParameters();
        parametros.Add("Resolucion", resolucion);

        if (cambios.Tipo != null)
        {
            asignaciones.Add("tipo = @Tipo");
            parametros.Add("Tipo", cambios.Tipo);
        }
        if (cambios.Calificacion != null)
        {
            asignaciones.Add("calificacion = @Calificacion");
            parametros.Add("Calificacion", cambios.Calificacion);
        }
        if (cambios.FechaInicio != null)
        {
            asignaciones.Add("fecha_inicio = @FechaInicio");
            parametros.Add("FechaInicio", cambios.FechaInicio);
        }
        if (cambios.FechaFin != null)
        {
            asignaciones.Add("fecha_fin = @FechaFin");
            parametros.Add("FechaFin", cambios.FechaFin);
        }
        if (cambios.Programa != null)
        {
            asignaciones.Add("programa = @Programa");
            parametros.Add("Programa", cambios.Programa);
        }

        if (asignaciones.Count == 0) return 0;

        var sql = $"UPDATE acreditacion SET {string.Join(", ", asignaciones)} WHERE resolucion = @Resolucion AND activo = 1";

        return await conexion.ExecuteAsync(sql, parametros);
    }

    public async Task<int> EliminarLogico(int resolucion)
    {
        using var conexion = CrearConexion();

        const string sql = @"
            UPDATE acreditacion
            SET activo = 0
            WHERE resolucion = @Resolucion AND activo = 1";

        return await conexion.ExecuteAsync(sql, new { Resolucion = resolucion });
    }
}
