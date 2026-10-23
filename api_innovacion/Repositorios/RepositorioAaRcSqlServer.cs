using System.Data;
using ApiInnovacion.Excepciones;
using ApiInnovacion.Modelos;
using Dapper;
using Microsoft.Data.SqlClient;

namespace ApiInnovacion.Repositorios;

/// <summary>
/// La capa 3 contra SQL Server, con Dapper, para aa_rc. El SQL se escribe
/// a mano, queda a la vista y SIEMPRE va parametrizado (@parametro).
///
/// Todas las consultas filtran por activo = 1: el borrado es lógico.
/// </summary>
public class RepositorioAaRcSqlServer : IRepositorioAaRc
{
    private readonly string _cadenaConexion;

    public RepositorioAaRcSqlServer(IConfiguration configuracion)
    {
        _cadenaConexion = configuracion.GetConnectionString("SqlServer")
            ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'SqlServer'.");
    }

    private IDbConnection CrearConexion() => new SqlConnection(_cadenaConexion);

    // Los alias traducen snake_case (tabla) a PascalCase (entidad): Dapper
    // mapea por nombre.
    private const string COLUMNAS = @"activ_academicas_idcurso AS ActivAcademicasIdcurso, registro_calificado_codigo AS RegistroCalificadoCodigo, componente AS Componente, semestre AS Semestre";

    public async Task<IEnumerable<AaRc>> ObtenerTodos(int limite)
    {
        using var conexion = CrearConexion();
        var sql = $@"
            SELECT TOP (@Limite) {COLUMNAS}
            FROM aa_rc
            WHERE activo = 1
            ORDER BY activ_academicas_idcurso ASC, registro_calificado_codigo ASC";

        return await conexion.QueryAsync<AaRc>(sql, new { Limite = limite });
    }

    public async Task<AaRc?> ObtenerPorLlave(int activAcademicasIdcurso, int registroCalificadoCodigo)
    {
        using var conexion = CrearConexion();
        // Un registro inactivo responde como inexistente.
        var sql = $@"
            SELECT {COLUMNAS}
            FROM aa_rc
            WHERE activ_academicas_idcurso = @ActivAcademicasIdcurso AND registro_calificado_codigo = @RegistroCalificadoCodigo AND activo = 1";

        return await conexion.QueryFirstOrDefaultAsync<AaRc>(sql, new { ActivAcademicasIdcurso = activAcademicasIdcurso, RegistroCalificadoCodigo = registroCalificadoCodigo });
    }

    public async Task Crear(AaRc entidad)
    {
        using var conexion = CrearConexion();
        const string sql = @"
            INSERT INTO aa_rc (activ_academicas_idcurso, registro_calificado_codigo, componente, semestre, activo)
            VALUES (@ActivAcademicasIdcurso, @RegistroCalificadoCodigo, @Componente, @Semestre, 1)";

        try
        {
            await conexion.ExecuteAsync(sql, entidad);
        }
        catch (SqlException ex) when (ex.Number is 2627 or 2601)
        {
            // 2627/2601: violación de llave primaria o índice único. Se traduce
            // aquí para que el servicio nunca vea un tipo de SqlClient.
            throw new LlaveDuplicadaExcepcion(
                "Ya existe un registro de aa_rc con esa llave (puede estar retirado).");
        }
    }

    public async Task<int> Reemplazar(AaRc entidad)
    {
        using var conexion = CrearConexion();
        const string sql = @"
            UPDATE aa_rc
            SET componente = @Componente, semestre = @Semestre
            WHERE activ_academicas_idcurso = @ActivAcademicasIdcurso AND registro_calificado_codigo = @RegistroCalificadoCodigo AND activo = 1";

        return await conexion.ExecuteAsync(sql, entidad);
    }

    public async Task<int> ActualizarParcial(int activAcademicasIdcurso, int registroCalificadoCodigo, AaRcCambios cambios)
    {
        using var conexion = CrearConexion();

        // El PATCH escribe solo lo que llegó, así que la consulta se compone.
        // Lo que se compone son NOMBRES DE COLUMNA de una lista cerrada escrita
        // aquí; los VALORES siempre viajan como @parametro.
        var asignaciones = new List<string>();
        var parametros = new DynamicParameters();
        parametros.Add("ActivAcademicasIdcurso", activAcademicasIdcurso);
        parametros.Add("RegistroCalificadoCodigo", registroCalificadoCodigo);

        if (cambios.Componente != null)
        {
            asignaciones.Add("componente = @Componente");
            parametros.Add("Componente", cambios.Componente);
        }
        if (cambios.Semestre != null)
        {
            asignaciones.Add("semestre = @Semestre");
            parametros.Add("Semestre", cambios.Semestre);
        }

        if (asignaciones.Count == 0) return 0;

        var sql = $"UPDATE aa_rc SET {string.Join(", ", asignaciones)} WHERE activ_academicas_idcurso = @ActivAcademicasIdcurso AND registro_calificado_codigo = @RegistroCalificadoCodigo AND activo = 1";

        return await conexion.ExecuteAsync(sql, parametros);
    }

    public async Task<int> EliminarLogico(int activAcademicasIdcurso, int registroCalificadoCodigo)
    {
        using var conexion = CrearConexion();
        // Borrado LÓGICO en una sola consulta: cero filas = no existe o ya
        // estaba inactivo, que es el 404 del contrato.
        const string sql = @"
            UPDATE aa_rc
            SET activo = 0
            WHERE activ_academicas_idcurso = @ActivAcademicasIdcurso AND registro_calificado_codigo = @RegistroCalificadoCodigo AND activo = 1";

        return await conexion.ExecuteAsync(sql, new { ActivAcademicasIdcurso = activAcademicasIdcurso, RegistroCalificadoCodigo = registroCalificadoCodigo });
    }
}
