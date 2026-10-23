using System.Data;
using ApiInnovacion.Excepciones;
using ApiInnovacion.Modelos;
using Dapper;
using Microsoft.Data.SqlClient;

namespace ApiInnovacion.Repositorios;

/// <summary>
/// La capa 3 contra SQL Server, con Dapper, para acreditacion. El SQL se escribe
/// a mano, queda a la vista y SIEMPRE va parametrizado (@parametro).
///
/// Todas las consultas filtran por activo = 1: el borrado es lógico.
/// </summary>
public class RepositorioAcreditacionSqlServer : IRepositorioAcreditacion
{
    private readonly string _cadenaConexion;

    public RepositorioAcreditacionSqlServer(IConfiguration configuracion)
    {
        _cadenaConexion = configuracion.GetConnectionString("SqlServer")
            ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'SqlServer'.");
    }

    private IDbConnection CrearConexion() => new SqlConnection(_cadenaConexion);

    // Los alias traducen snake_case (tabla) a PascalCase (entidad): Dapper
    // mapea por nombre.
    private const string COLUMNAS = @"resolucion AS Resolucion, tipo AS Tipo, calificacion AS Calificacion, fecha_inicio AS FechaInicio, fecha_fin AS FechaFin, programa AS Programa";

    public async Task<IEnumerable<Acreditacion>> ObtenerTodos(int limite)
    {
        using var conexion = CrearConexion();
        var sql = $@"
            SELECT TOP (@Limite) {COLUMNAS}
            FROM acreditacion
            WHERE activo = 1
            ORDER BY resolucion ASC";

        return await conexion.QueryAsync<Acreditacion>(sql, new { Limite = limite });
    }

    public async Task<Acreditacion?> ObtenerPorResolucion(int resolucion)
    {
        using var conexion = CrearConexion();
        // Un registro inactivo responde como inexistente.
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
        catch (SqlException ex) when (ex.Number is 2627 or 2601)
        {
            // 2627/2601: violación de llave primaria o índice único. Se traduce
            // aquí para que el servicio nunca vea un tipo de SqlClient.
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

        // El PATCH escribe solo lo que llegó, así que la consulta se compone.
        // Lo que se compone son NOMBRES DE COLUMNA de una lista cerrada escrita
        // aquí; los VALORES siempre viajan como @parametro.
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
        // Borrado LÓGICO en una sola consulta: cero filas = no existe o ya
        // estaba inactivo, que es el 404 del contrato.
        const string sql = @"
            UPDATE acreditacion
            SET activo = 0
            WHERE resolucion = @Resolucion AND activo = 1";

        return await conexion.ExecuteAsync(sql, new { Resolucion = resolucion });
    }
}
