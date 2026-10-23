using System.Data;
using ApiInnovacion.Excepciones;
using ApiInnovacion.Modelos;
using Dapper;
using Microsoft.Data.SqlClient;

namespace ApiInnovacion.Repositorios;

/// <summary>
/// La capa 3 contra SQL Server, con Dapper, para docente_departamento. El SQL se escribe
/// a mano, queda a la vista y SIEMPRE va parametrizado (@parametro).
///
/// Todas las consultas filtran por activo = 1: el borrado es lógico.
/// </summary>
public class RepositorioDocenteDepartamentoSqlServer : IRepositorioDocenteDepartamento
{
    private readonly string _cadenaConexion;

    public RepositorioDocenteDepartamentoSqlServer(IConfiguration configuracion)
    {
        _cadenaConexion = configuracion.GetConnectionString("SqlServer")
            ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'SqlServer'.");
    }

    private IDbConnection CrearConexion() => new SqlConnection(_cadenaConexion);

    // Los alias traducen snake_case (tabla) a PascalCase (entidad): Dapper
    // mapea por nombre.
    private const string COLUMNAS = @"docente AS Docente, departamento AS Departamento, dedicacion AS Dedicacion, modalidad AS Modalidad, fecha_ingreso AS FechaIngreso, fecha_salida AS FechaSalida";

    public async Task<IEnumerable<DocenteDepartamento>> ObtenerTodos(int limite)
    {
        using var conexion = CrearConexion();
        var sql = $@"
            SELECT TOP (@Limite) {COLUMNAS}
            FROM docente_departamento
            WHERE activo = 1
            ORDER BY docente ASC, departamento ASC";

        return await conexion.QueryAsync<DocenteDepartamento>(sql, new { Limite = limite });
    }

    public async Task<DocenteDepartamento?> ObtenerPorLlave(int docente, int departamento)
    {
        using var conexion = CrearConexion();
        // Un registro inactivo responde como inexistente.
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
        catch (SqlException ex) when (ex.Number is 2627 or 2601)
        {
            // 2627/2601: violación de llave primaria o índice único. Se traduce
            // aquí para que el servicio nunca vea un tipo de SqlClient.
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

        // El PATCH escribe solo lo que llegó, así que la consulta se compone.
        // Lo que se compone son NOMBRES DE COLUMNA de una lista cerrada escrita
        // aquí; los VALORES siempre viajan como @parametro.
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
        // Borrado LÓGICO en una sola consulta: cero filas = no existe o ya
        // estaba inactivo, que es el 404 del contrato.
        const string sql = @"
            UPDATE docente_departamento
            SET activo = 0
            WHERE docente = @Docente AND departamento = @Departamento AND activo = 1";

        return await conexion.ExecuteAsync(sql, new { Docente = docente, Departamento = departamento });
    }
}
