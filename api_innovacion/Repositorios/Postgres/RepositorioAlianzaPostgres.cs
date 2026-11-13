// PostgreSQL: consultas explícitas; conserva el contrato del repositorio SQL Server.
using System.Data;
using ApiInnovacion.Excepciones;
using ApiInnovacion.Modelos;
using Dapper;
using Npgsql;

namespace ApiInnovacion.Repositorios.Postgres;

public class RepositorioAlianzaPostgres : IRepositorioAlianza
{
    private readonly string _cadenaConexion;

    public RepositorioAlianzaPostgres(ConfiguracionPostgres configuracion)
    {
        _cadenaConexion = configuracion.CadenaConexion;
    }

    private IDbConnection CrearConexion() => new NpgsqlConnection(_cadenaConexion);

    private const string COLUMNAS = @"aliado AS Aliado, departamento AS Departamento, fecha_inicio::timestamp AS FechaInicio, fecha_fin::timestamp AS FechaFin, docente AS Docente";

    public async Task<IEnumerable<Alianza>> ObtenerTodos(int limite)
    {
        using var conexion = CrearConexion();
        var sql = $@"
            SELECT {COLUMNAS}
            FROM alianza
            WHERE activo = 1
            ORDER BY aliado ASC, departamento ASC LIMIT @Limite";

        return await conexion.QueryAsync<Alianza>(sql, new { Limite = limite });
    }

    public async Task<Alianza?> ObtenerPorLlave(int aliado, int departamento)
    {
        using var conexion = CrearConexion();

        var sql = $@"
            SELECT {COLUMNAS}
            FROM alianza
            WHERE aliado = @Aliado AND departamento = @Departamento AND activo = 1";

        return await conexion.QueryFirstOrDefaultAsync<Alianza>(sql, new { Aliado = aliado, Departamento = departamento });
    }

    public async Task Crear(Alianza entidad)
    {
        using var conexion = CrearConexion();
        const string sql = @"
            INSERT INTO alianza (aliado, departamento, fecha_inicio, fecha_fin, docente, activo)
            VALUES (@Aliado, @Departamento, @FechaInicio, @FechaFin, @Docente, 1)";

        try
        {
            await conexion.ExecuteAsync(sql, entidad);
        }
        catch (PostgresException ex) when (ex.SqlState == "23505")
        {

            throw new LlaveDuplicadaExcepcion(
                "Ya existe un registro de alianza con esa llave (puede estar retirado).");
        }
    }

    public async Task<int> Reemplazar(Alianza entidad)
    {
        using var conexion = CrearConexion();
        const string sql = @"
            UPDATE alianza
            SET fecha_inicio = @FechaInicio, fecha_fin = @FechaFin, docente = @Docente
            WHERE aliado = @Aliado AND departamento = @Departamento AND activo = 1";

        return await conexion.ExecuteAsync(sql, entidad);
    }

    public async Task<int> ActualizarParcial(int aliado, int departamento, AlianzaCambios cambios)
    {
        using var conexion = CrearConexion();

        var asignaciones = new List<string>();
        var parametros = new DynamicParameters();
        parametros.Add("Aliado", aliado);
        parametros.Add("Departamento", departamento);

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
        if (cambios.Docente != null)
        {
            asignaciones.Add("docente = @Docente");
            parametros.Add("Docente", cambios.Docente);
        }

        if (asignaciones.Count == 0) return 0;

        var sql = $"UPDATE alianza SET {string.Join(", ", asignaciones)} WHERE aliado = @Aliado AND departamento = @Departamento AND activo = 1";

        return await conexion.ExecuteAsync(sql, parametros);
    }

    public async Task<int> EliminarLogico(int aliado, int departamento)
    {
        using var conexion = CrearConexion();

        const string sql = @"
            UPDATE alianza
            SET activo = 0
            WHERE aliado = @Aliado AND departamento = @Departamento AND activo = 1";

        return await conexion.ExecuteAsync(sql, new { Aliado = aliado, Departamento = departamento });
    }
}
