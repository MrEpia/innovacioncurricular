using System.Data;
using ApiInnovacion.Excepciones;
using ApiInnovacion.Modelos;
using Dapper;
using Microsoft.Data.SqlClient;

namespace ApiInnovacion.Repositorios;

/// <summary>
/// La capa 3 contra SQL Server, con Dapper, para alianza. El SQL se escribe
/// a mano, queda a la vista y SIEMPRE va parametrizado (@parametro).
///
/// Todas las consultas filtran por activo = 1: el borrado es lógico.
/// </summary>
public class RepositorioAlianzaSqlServer : IRepositorioAlianza
{
    private readonly string _cadenaConexion;

    public RepositorioAlianzaSqlServer(IConfiguration configuracion)
    {
        _cadenaConexion = configuracion.GetConnectionString("SqlServer")
            ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'SqlServer'.");
    }

    private IDbConnection CrearConexion() => new SqlConnection(_cadenaConexion);

    // Los alias traducen snake_case (tabla) a PascalCase (entidad): Dapper
    // mapea por nombre.
    private const string COLUMNAS = @"aliado AS Aliado, departamento AS Departamento, fecha_inicio AS FechaInicio, fecha_fin AS FechaFin, docente AS Docente";

    public async Task<IEnumerable<Alianza>> ObtenerTodos(int limite)
    {
        using var conexion = CrearConexion();
        var sql = $@"
            SELECT TOP (@Limite) {COLUMNAS}
            FROM alianza
            WHERE activo = 1
            ORDER BY aliado ASC, departamento ASC";

        return await conexion.QueryAsync<Alianza>(sql, new { Limite = limite });
    }

    public async Task<Alianza?> ObtenerPorLlave(int aliado, int departamento)
    {
        using var conexion = CrearConexion();
        // Un registro inactivo responde como inexistente.
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
        catch (SqlException ex) when (ex.Number is 2627 or 2601)
        {
            // 2627/2601: violación de llave primaria o índice único. Se traduce
            // aquí para que el servicio nunca vea un tipo de SqlClient.
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

        // El PATCH escribe solo lo que llegó, así que la consulta se compone.
        // Lo que se compone son NOMBRES DE COLUMNA de una lista cerrada escrita
        // aquí; los VALORES siempre viajan como @parametro.
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
        // Borrado LÓGICO en una sola consulta: cero filas = no existe o ya
        // estaba inactivo, que es el 404 del contrato.
        const string sql = @"
            UPDATE alianza
            SET activo = 0
            WHERE aliado = @Aliado AND departamento = @Departamento AND activo = 1";

        return await conexion.ExecuteAsync(sql, new { Aliado = aliado, Departamento = departamento });
    }
}
