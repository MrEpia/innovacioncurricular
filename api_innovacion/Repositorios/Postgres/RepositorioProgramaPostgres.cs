// PostgreSQL: consultas explícitas; conserva el contrato del repositorio SQL Server.
using System.Data;
using ApiInnovacion.Excepciones;
using ApiInnovacion.Modelos;
using Dapper;
using Npgsql;

namespace ApiInnovacion.Repositorios.Postgres;

public class RepositorioProgramaPostgres : IRepositorioPrograma
{
    private readonly string _cadenaConexion;

    public RepositorioProgramaPostgres(ConfiguracionPostgres configuracion)
    {
        _cadenaConexion = configuracion.CadenaConexion;
    }

    private IDbConnection CrearConexion() => new NpgsqlConnection(_cadenaConexion);

    private const string COLUMNAS = @"id AS Id, nombre AS Nombre, tipo AS Tipo, nivel AS Nivel, fecha_creacion AS FechaCreacion, fecha_cierre AS FechaCierre, numero_cohortes AS NumeroCohortes, cant_graduados AS CantGraduados, fecha_actualizacion AS FechaActualizacion, ciudad AS Ciudad, facultad AS Facultad";

    public async Task<IEnumerable<Programa>> ObtenerTodos(int limite)
    {
        using var conexion = CrearConexion();
        var sql = $@"
            SELECT {COLUMNAS}
            FROM programa
            WHERE activo = 1
            ORDER BY id ASC LIMIT @Limite";

        return await conexion.QueryAsync<Programa>(sql, new { Limite = limite });
    }

    public async Task<Programa?> ObtenerPorId(int id)
    {
        using var conexion = CrearConexion();

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
        catch (PostgresException ex) when (ex.SqlState == "23505")
        {

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

        try
        {
            return await conexion.ExecuteScalarAsync<int>(
                "SELECT * FROM public.sp_retirar_programa(@Id)", new { Id = id });
        }
        catch (PostgresException ex) when (ex.SqlState == "P0409")
        {
            throw new DependenciasActivasExcepcion(
                $"No se puede retirar el programa {id}: {ex.MessageText}. Retírelos primero.");
        }
    }
}
