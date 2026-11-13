// PostgreSQL: consultas explícitas; conserva el contrato del repositorio SQL Server.
using System.Data;
using ApiInnovacion.Excepciones;
using ApiInnovacion.Modelos;
using Dapper;
using Npgsql;

namespace ApiInnovacion.Repositorios.Postgres;

public class RepositorioDocenteDepartamentoPostgres : IRepositorioDocenteDepartamento
{
    private readonly string _cadenaConexion;

    public RepositorioDocenteDepartamentoPostgres(ConfiguracionPostgres configuracion)
    {
        _cadenaConexion = configuracion.CadenaConexion;
    }

    private IDbConnection CrearConexion() => new NpgsqlConnection(_cadenaConexion);

    private const string COLUMNAS = @"docente AS Docente, departamento AS Departamento, dedicacion AS Dedicacion, modalidad AS Modalidad, fecha_ingreso::timestamp AS FechaIngreso, fecha_salida::timestamp AS FechaSalida";

    public async Task<IEnumerable<DocenteDepartamento>> ObtenerTodos(int limite)
    {
        using var conexion = CrearConexion();
        var sql = $@"
            SELECT {COLUMNAS}
            FROM docente_departamento
            WHERE activo = 1
            ORDER BY docente ASC, departamento ASC LIMIT @Limite";

        return await conexion.QueryAsync<DocenteDepartamento>(sql, new { Limite = limite });
    }

    public async Task<DocenteDepartamento?> ObtenerPorLlave(int docente, int departamento)
    {
        using var conexion = CrearConexion();

        var sql = $@"
            SELECT {COLUMNAS}
            FROM docente_departamento
            WHERE docente = @Docente AND departamento = @Departamento AND activo = 1";

        return await conexion.QueryFirstOrDefaultAsync<DocenteDepartamento>(sql, new { Docente = docente, Departamento = departamento });
    }

    public async Task Crear(DocenteDepartamento entidad)
    {
        using var conexion = CrearConexion();
        const string sql = @"
            INSERT INTO docente_departamento (docente, departamento, dedicacion, modalidad, fecha_ingreso, fecha_salida, activo)
            VALUES (@Docente, @Departamento, @Dedicacion, @Modalidad, @FechaIngreso, @FechaSalida, 1)";

        try
        {
            await conexion.ExecuteAsync(sql, entidad);
        }
        catch (PostgresException ex) when (ex.SqlState == "23505")
        {

            throw new LlaveDuplicadaExcepcion(
                "Ya existe un registro de docente_departamento con esa llave (puede estar retirado).");
        }
    }

    public async Task<int> Reemplazar(DocenteDepartamento entidad)
    {
        using var conexion = CrearConexion();
        const string sql = @"
            UPDATE docente_departamento
            SET dedicacion = @Dedicacion, modalidad = @Modalidad, fecha_ingreso = @FechaIngreso, fecha_salida = @FechaSalida
            WHERE docente = @Docente AND departamento = @Departamento AND activo = 1";

        return await conexion.ExecuteAsync(sql, entidad);
    }

    public async Task<int> ActualizarParcial(int docente, int departamento, DocenteDepartamentoCambios cambios)
    {
        using var conexion = CrearConexion();

        var asignaciones = new List<string>();
        var parametros = new DynamicParameters();
        parametros.Add("Docente", docente);
        parametros.Add("Departamento", departamento);

        if (cambios.Dedicacion != null)
        {
            asignaciones.Add("dedicacion = @Dedicacion");
            parametros.Add("Dedicacion", cambios.Dedicacion);
        }
        if (cambios.Modalidad != null)
        {
            asignaciones.Add("modalidad = @Modalidad");
            parametros.Add("Modalidad", cambios.Modalidad);
        }
        if (cambios.FechaIngreso != null)
        {
            asignaciones.Add("fecha_ingreso = @FechaIngreso");
            parametros.Add("FechaIngreso", cambios.FechaIngreso);
        }
        if (cambios.FechaSalida != null)
        {
            asignaciones.Add("fecha_salida = @FechaSalida");
            parametros.Add("FechaSalida", cambios.FechaSalida);
        }

        if (asignaciones.Count == 0) return 0;

        var sql = $"UPDATE docente_departamento SET {string.Join(", ", asignaciones)} WHERE docente = @Docente AND departamento = @Departamento AND activo = 1";

        return await conexion.ExecuteAsync(sql, parametros);
    }

    public async Task<int> EliminarLogico(int docente, int departamento)
    {
        using var conexion = CrearConexion();

        const string sql = @"
            UPDATE docente_departamento
            SET activo = 0
            WHERE docente = @Docente AND departamento = @Departamento AND activo = 1";

        return await conexion.ExecuteAsync(sql, new { Docente = docente, Departamento = departamento });
    }
}
