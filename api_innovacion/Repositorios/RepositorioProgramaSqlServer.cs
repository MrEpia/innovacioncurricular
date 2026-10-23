using System.Data;
using ApiInnovacion.Excepciones;
using ApiInnovacion.Modelos;
using Dapper;
using Microsoft.Data.SqlClient;

namespace ApiInnovacion.Repositorios;

/// <summary>
/// La capa 3 contra SQL Server, con Dapper, para programa. El SQL se escribe
/// a mano, queda a la vista y SIEMPRE va parametrizado (@parametro).
///
/// Todas las consultas filtran por activo = 1: el borrado es lógico.
/// </summary>
public class RepositorioProgramaSqlServer : IRepositorioPrograma
{
    private readonly string _cadenaConexion;

    public RepositorioProgramaSqlServer(IConfiguration configuracion)
    {
        _cadenaConexion = configuracion.GetConnectionString("SqlServer")
            ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'SqlServer'.");
    }

    private IDbConnection CrearConexion() => new SqlConnection(_cadenaConexion);

    // Los alias traducen snake_case (tabla) a PascalCase (entidad): Dapper
    // mapea por nombre.
    private const string COLUMNAS = @"id AS Id, nombre AS Nombre, tipo AS Tipo, nivel AS Nivel, fecha_creacion AS FechaCreacion, fecha_cierre AS FechaCierre, numero_cohortes AS NumeroCohortes, cant_graduados AS CantGraduados, fecha_actualizacion AS FechaActualizacion, ciudad AS Ciudad, facultad AS Facultad";

    public async Task<IEnumerable<Programa>> ObtenerTodos(int limite)
    {
        using var conexion = CrearConexion();
        var sql = $@"
            SELECT TOP (@Limite) {COLUMNAS}
            FROM programa
            WHERE activo = 1
            ORDER BY id ASC";

        return await conexion.QueryAsync<Programa>(sql, new { Limite = limite });
    }

    public async Task<Programa?> ObtenerPorId(int id)
    {
        using var conexion = CrearConexion();
        // Un registro inactivo responde como inexistente.
        var sql = $@"
            SELECT {COLUMNAS}
            FROM programa
            WHERE id = @Id AND activo = 1";

        return await conexion.QueryFirstOrDefaultAsync<Programa>(sql, new { Id = id });
    }

    public async Task Crear(Programa entidad)
    {
        using var conexion = CrearConexion();
        const string sql = @"
            INSERT INTO programa (id, nombre, tipo, nivel, fecha_creacion, fecha_cierre, numero_cohortes, cant_graduados, fecha_actualizacion, ciudad, facultad, activo)
            VALUES (@Id, @Nombre, @Tipo, @Nivel, @FechaCreacion, @FechaCierre, @NumeroCohortes, @CantGraduados, @FechaActualizacion, @Ciudad, @Facultad, 1)";

        try
        {
            await conexion.ExecuteAsync(sql, entidad);
        }
        catch (SqlException ex) when (ex.Number is 2627 or 2601)
        {
            // 2627/2601: violación de llave primaria o índice único. Se traduce
            // aquí para que el servicio nunca vea un tipo de SqlClient.
            throw new LlaveDuplicadaExcepcion(
                "Ya existe un registro de programa con esa llave (puede estar retirado).");
        }
    }

    public async Task<int> Reemplazar(Programa entidad)
    {
        using var conexion = CrearConexion();
        const string sql = @"
            UPDATE programa
            SET nombre = @Nombre, tipo = @Tipo, nivel = @Nivel, fecha_creacion = @FechaCreacion, fecha_cierre = @FechaCierre, numero_cohortes = @NumeroCohortes, cant_graduados = @CantGraduados, fecha_actualizacion = @FechaActualizacion, ciudad = @Ciudad, facultad = @Facultad
            WHERE id = @Id AND activo = 1";

        return await conexion.ExecuteAsync(sql, entidad);
    }

    public async Task<int> ActualizarParcial(int id, ProgramaCambios cambios)
    {
        using var conexion = CrearConexion();

        // El PATCH escribe solo lo que llegó, así que la consulta se compone.
        // Lo que se compone son NOMBRES DE COLUMNA de una lista cerrada escrita
        // aquí; los VALORES siempre viajan como @parametro.
        var asignaciones = new List<string>();
        var parametros = new DynamicParameters();
        parametros.Add("Id", id);

        if (cambios.Nombre != null)
        {
            asignaciones.Add("nombre = @Nombre");
            parametros.Add("Nombre", cambios.Nombre);
        }
        if (cambios.Tipo != null)
        {
            asignaciones.Add("tipo = @Tipo");
            parametros.Add("Tipo", cambios.Tipo);
        }
        if (cambios.Nivel != null)
        {
            asignaciones.Add("nivel = @Nivel");
            parametros.Add("Nivel", cambios.Nivel);
        }
        if (cambios.FechaCreacion != null)
        {
            asignaciones.Add("fecha_creacion = @FechaCreacion");
            parametros.Add("FechaCreacion", cambios.FechaCreacion);
        }
        if (cambios.FechaCierre != null)
        {
            asignaciones.Add("fecha_cierre = @FechaCierre");
            parametros.Add("FechaCierre", cambios.FechaCierre);
        }
        if (cambios.NumeroCohortes != null)
        {
            asignaciones.Add("numero_cohortes = @NumeroCohortes");
            parametros.Add("NumeroCohortes", cambios.NumeroCohortes);
        }
        if (cambios.CantGraduados != null)
        {
            asignaciones.Add("cant_graduados = @CantGraduados");
            parametros.Add("CantGraduados", cambios.CantGraduados);
        }
        if (cambios.FechaActualizacion != null)
        {
            asignaciones.Add("fecha_actualizacion = @FechaActualizacion");
            parametros.Add("FechaActualizacion", cambios.FechaActualizacion);
        }
        if (cambios.Ciudad != null)
        {
            asignaciones.Add("ciudad = @Ciudad");
            parametros.Add("Ciudad", cambios.Ciudad);
        }
        if (cambios.Facultad != null)
        {
            asignaciones.Add("facultad = @Facultad");
            parametros.Add("Facultad", cambios.Facultad);
        }

        if (asignaciones.Count == 0) return 0;

        var sql = $"UPDATE programa SET {string.Join(", ", asignaciones)} WHERE id = @Id AND activo = 1";

        return await conexion.ExecuteAsync(sql, parametros);
    }

    public async Task<int> EliminarLogico(int id)
    {
        using var conexion = CrearConexion();
        // Procedimiento sp_retirar_programa (migración v3_002): comprueba que no
        // tenga hijos activos y lo retira en la MISMA transacción, para que
        // nadie le cuelgue un hijo entre la comprobación y el retiro.
        // Cero filas = no existe o ya estaba inactivo (404).
        try
        {
            return await conexion.ExecuteScalarAsync<int>(
                "dbo.sp_retirar_programa", new { Id = id }, commandType: CommandType.StoredProcedure);
        }
        catch (SqlException ex) when (ex.Number == 50409)
        {
            throw new DependenciasActivasExcepcion(
                $"No se puede retirar el programa {id}: {ex.Message}. Retírelos primero.");
        }
    }
}
