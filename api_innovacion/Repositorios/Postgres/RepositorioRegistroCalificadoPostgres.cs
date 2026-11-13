// PostgreSQL: consultas explícitas; conserva el contrato del repositorio SQL Server.
using System.Data;
using ApiInnovacion.Excepciones;
using ApiInnovacion.Modelos;
using Dapper;
using Npgsql;

namespace ApiInnovacion.Repositorios.Postgres;

public class RepositorioRegistroCalificadoPostgres : IRepositorioRegistroCalificado
{
    private readonly string _cadenaConexion;

    public RepositorioRegistroCalificadoPostgres(ConfiguracionPostgres configuracion)
    {
        _cadenaConexion = configuracion.CadenaConexion;
    }

    private IDbConnection CrearConexion() => new NpgsqlConnection(_cadenaConexion);

    private const string COLUMNAS = @"codigo AS Codigo, cant_creditos AS CantCreditos, hora_acom AS HoraAcom, hora_ind AS HoraInd, metodologia AS Metodologia, fecha_inicio::timestamp AS FechaInicio, fecha_fin::timestamp AS FechaFin, duracion_anios AS DuracionAnios, duracion_semestres AS DuracionSemestres, tipo_titulacion AS TipoTitulacion, programa AS Programa";

    public async Task<IEnumerable<RegistroCalificado>> ObtenerTodos(int limite)
    {
        using var conexion = CrearConexion();
        var sql = $@"
            SELECT {COLUMNAS}
            FROM registro_calificado
            WHERE activo = 1
            ORDER BY codigo ASC LIMIT @Limite";

        return await conexion.QueryAsync<RegistroCalificado>(sql, new { Limite = limite });
    }

    public async Task<RegistroCalificado?> ObtenerPorCodigo(int codigo)
    {
        using var conexion = CrearConexion();

        var sql = $@"
            SELECT {COLUMNAS}
            FROM registro_calificado
            WHERE codigo = @Codigo AND activo = 1";

        return await conexion.QueryFirstOrDefaultAsync<RegistroCalificado>(sql, new { Codigo = codigo });
    }

    public async Task Crear(RegistroCalificado entidad)
    {
        using var conexion = CrearConexion();
        const string sql = @"
            INSERT INTO registro_calificado (codigo, cant_creditos, hora_acom, hora_ind, metodologia, fecha_inicio, fecha_fin, duracion_anios, duracion_semestres, tipo_titulacion, programa, activo)
            VALUES (@Codigo, @CantCreditos, @HoraAcom, @HoraInd, @Metodologia, @FechaInicio, @FechaFin, @DuracionAnios, @DuracionSemestres, @TipoTitulacion, @Programa, 1)";

        try
        {
            await conexion.ExecuteAsync(sql, entidad);
        }
        catch (PostgresException ex) when (ex.SqlState == "23505")
        {

            throw new LlaveDuplicadaExcepcion(
                "Ya existe un registro de registro_calificado con esa llave (puede estar retirado).");
        }
    }

    public async Task<int> Reemplazar(RegistroCalificado entidad)
    {
        using var conexion = CrearConexion();
        const string sql = @"
            UPDATE registro_calificado
            SET cant_creditos = @CantCreditos, hora_acom = @HoraAcom, hora_ind = @HoraInd, metodologia = @Metodologia, fecha_inicio = @FechaInicio, fecha_fin = @FechaFin, duracion_anios = @DuracionAnios, duracion_semestres = @DuracionSemestres, tipo_titulacion = @TipoTitulacion, programa = @Programa
            WHERE codigo = @Codigo AND activo = 1";

        return await conexion.ExecuteAsync(sql, entidad);
    }

    public async Task<int> ActualizarParcial(int codigo, RegistroCalificadoCambios cambios)
    {
        using var conexion = CrearConexion();

        var asignaciones = new List<string>();
        var parametros = new DynamicParameters();
        parametros.Add("Codigo", codigo);

        if (cambios.CantCreditos != null)
        {
            asignaciones.Add("cant_creditos = @CantCreditos");
            parametros.Add("CantCreditos", cambios.CantCreditos);
        }
        if (cambios.HoraAcom != null)
        {
            asignaciones.Add("hora_acom = @HoraAcom");
            parametros.Add("HoraAcom", cambios.HoraAcom);
        }
        if (cambios.HoraInd != null)
        {
            asignaciones.Add("hora_ind = @HoraInd");
            parametros.Add("HoraInd", cambios.HoraInd);
        }
        if (cambios.Metodologia != null)
        {
            asignaciones.Add("metodologia = @Metodologia");
            parametros.Add("Metodologia", cambios.Metodologia);
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
        if (cambios.DuracionAnios != null)
        {
            asignaciones.Add("duracion_anios = @DuracionAnios");
            parametros.Add("DuracionAnios", cambios.DuracionAnios);
        }
        if (cambios.DuracionSemestres != null)
        {
            asignaciones.Add("duracion_semestres = @DuracionSemestres");
            parametros.Add("DuracionSemestres", cambios.DuracionSemestres);
        }
        if (cambios.TipoTitulacion != null)
        {
            asignaciones.Add("tipo_titulacion = @TipoTitulacion");
            parametros.Add("TipoTitulacion", cambios.TipoTitulacion);
        }
        if (cambios.Programa != null)
        {
            asignaciones.Add("programa = @Programa");
            parametros.Add("Programa", cambios.Programa);
        }

        if (asignaciones.Count == 0) return 0;

        var sql = $"UPDATE registro_calificado SET {string.Join(", ", asignaciones)} WHERE codigo = @Codigo AND activo = 1";

        return await conexion.ExecuteAsync(sql, parametros);
    }

    public async Task<int> EliminarLogico(int codigo)
    {
        using var conexion = CrearConexion();

        try
        {
            return await conexion.ExecuteScalarAsync<int>(
                "SELECT * FROM public.sp_retirar_registro_calificado(@Codigo)", new { Codigo = codigo });
        }
        catch (PostgresException ex) when (ex.SqlState == "P0409")
        {
            throw new DependenciasActivasExcepcion(
                $"No se puede retirar el registro calificado {codigo}: {ex.MessageText}. Retírelos primero.");
        }
    }
}
