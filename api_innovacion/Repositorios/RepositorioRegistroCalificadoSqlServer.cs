using System.Data;
using ApiInnovacion.Excepciones;
using ApiInnovacion.Modelos;
using Dapper;
using Microsoft.Data.SqlClient;

namespace ApiInnovacion.Repositorios;

/// <summary>
/// La capa 3 contra SQL Server, con Dapper, para registro_calificado. El SQL se escribe
/// a mano, queda a la vista y SIEMPRE va parametrizado (@parametro).
///
/// Todas las consultas filtran por activo = 1: el borrado es lógico.
/// </summary>
public class RepositorioRegistroCalificadoSqlServer : IRepositorioRegistroCalificado
{
    private readonly string _cadenaConexion;

    public RepositorioRegistroCalificadoSqlServer(IConfiguration configuracion)
    {
        _cadenaConexion = configuracion.GetConnectionString("SqlServer")
            ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'SqlServer'.");
    }

    private IDbConnection CrearConexion() => new SqlConnection(_cadenaConexion);

    // Los alias traducen snake_case (tabla) a PascalCase (entidad): Dapper
    // mapea por nombre.
    private const string COLUMNAS = @"codigo AS Codigo, cant_creditos AS CantCreditos, hora_acom AS HoraAcom, hora_ind AS HoraInd, metodologia AS Metodologia, fecha_inicio AS FechaInicio, fecha_fin AS FechaFin, duracion_anios AS DuracionAnios, duracion_semestres AS DuracionSemestres, tipo_titulacion AS TipoTitulacion, programa AS Programa";

    public async Task<IEnumerable<RegistroCalificado>> ObtenerTodos(int limite)
    {
        using var conexion = CrearConexion();
        var sql = $@"
            SELECT TOP (@Limite) {COLUMNAS}
            FROM registro_calificado
            WHERE activo = 1
            ORDER BY codigo ASC";

        return await conexion.QueryAsync<RegistroCalificado>(sql, new { Limite = limite });
    }

    public async Task<RegistroCalificado?> ObtenerPorCodigo(int codigo)
    {
        using var conexion = CrearConexion();
        // Un registro inactivo responde como inexistente.
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
        catch (SqlException ex) when (ex.Number is 2627 or 2601)
        {
            // 2627/2601: violación de llave primaria o índice único. Se traduce
            // aquí para que el servicio nunca vea un tipo de SqlClient.
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

        // El PATCH escribe solo lo que llegó, así que la consulta se compone.
        // Lo que se compone son NOMBRES DE COLUMNA de una lista cerrada escrita
        // aquí; los VALORES siempre viajan como @parametro.
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
        // Borrado LÓGICO en una sola consulta: cero filas = no existe o ya
        // estaba inactivo, que es el 404 del contrato.
        const string sql = @"
            UPDATE registro_calificado
            SET activo = 0
            WHERE codigo = @Codigo AND activo = 1";

        return await conexion.ExecuteAsync(sql, new { Codigo = codigo });
    }
}
