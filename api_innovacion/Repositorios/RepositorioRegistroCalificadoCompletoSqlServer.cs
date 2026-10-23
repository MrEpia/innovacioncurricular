using System.Data;
using System.Text.Json;
using ApiInnovacion.Excepciones;
using ApiInnovacion.Modelos;
using Dapper;
using Microsoft.Data.SqlClient;

namespace ApiInnovacion.Repositorios;

/// <summary>
/// El maestro-detalle de registro_calificado contra SQL Server.
///
/// Escribir: un solo viaje al procedimiento sp_registro_calificado_completo_crear
/// (migración v3_003). El detalle viaja como JSON en un parámetro y el
/// procedimiento lo inserta con OPENJSON dentro de UNA transacción: si la
/// tercera actividad falla, el registro y los dos primeros hijos tampoco quedan.
///
/// Leer: tres consultas parametrizadas en un mismo viaje (QueryMultiple).
/// </summary>
public class RepositorioRegistroCalificadoCompletoSqlServer : IRepositorioRegistroCalificadoCompleto
{
    private readonly string _cadenaConexion;

    public RepositorioRegistroCalificadoCompletoSqlServer(IConfiguration configuracion)
    {
        _cadenaConexion = configuracion.GetConnectionString("SqlServer")
            ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'SqlServer'.");
    }

    private IDbConnection CrearConexion() => new SqlConnection(_cadenaConexion);

    public async Task Crear(RegistroCalificadoCompleto entidad)
    {
        using var conexion = CrearConexion();
        var parametros = new
        {
            entidad.Codigo,
            entidad.CantCreditos,
            entidad.HoraAcom,
            entidad.HoraInd,
            entidad.Metodologia,
            entidad.FechaInicio,
            entidad.FechaFin,
            entidad.DuracionAnios,
            entidad.DuracionSemestres,
            entidad.TipoTitulacion,
            entidad.Programa,
            Enfoques = JsonSerializer.Serialize(entidad.Enfoques.Select(e => e.Enfoque)),
            Actividades = JsonSerializer.Serialize(entidad.Actividades.Select(a => new
            {
                id = a.ActivAcademicasIdcurso,
                componente = a.Componente,
                semestre = a.Semestre
            }))
        };

        try
        {
            await conexion.ExecuteAsync("dbo.sp_registro_calificado_completo_crear", parametros,
                commandType: CommandType.StoredProcedure);
        }
        catch (SqlException ex) when (ex.Number is 2627 or 2601)
        {
            throw new LlaveDuplicadaExcepcion(
                "Ya existe un registro de registro_calificado con esa llave (puede estar retirado). No se guardó nada.");
        }
        catch (SqlException ex) when (ex.Number == 50400)
        {
            // Un padre se retiró entre la validación del servicio y la escritura.
            throw new ReferenciaInvalidaExcepcion(ex.Message);
        }
    }

    public async Task<RegistroCalificadoCompleto?> ObtenerCompleto(int codigo)
    {
        using var conexion = CrearConexion();
        const string sql = @"
            SELECT codigo AS Codigo, cant_creditos AS CantCreditos, hora_acom AS HoraAcom, hora_ind AS HoraInd,
                   metodologia AS Metodologia, fecha_inicio AS FechaInicio, fecha_fin AS FechaFin,
                   duracion_anios AS DuracionAnios, duracion_semestres AS DuracionSemestres,
                   tipo_titulacion AS TipoTitulacion, programa AS Programa
            FROM registro_calificado
            WHERE codigo = @Codigo AND activo = 1;

            SELECT er.enfoque AS Enfoque, e.nombre AS Nombre
            FROM enfoque_rc er
            JOIN enfoque e ON e.id = er.enfoque
            WHERE er.registro_calificado = @Codigo AND er.activo = 1
            ORDER BY er.enfoque;

            SELECT aa.activ_academicas_idcurso AS ActivAcademicasIdcurso, aa.componente AS Componente,
                   aa.semestre AS Semestre, a.nombre AS Nombre
            FROM aa_rc aa
            JOIN activ_academica a ON a.id = aa.activ_academicas_idcurso
            WHERE aa.registro_calificado_codigo = @Codigo AND aa.activo = 1
            ORDER BY aa.activ_academicas_idcurso;";

        using var lector = await conexion.QueryMultipleAsync(sql, new { Codigo = codigo });
        var maestro = await lector.ReadFirstOrDefaultAsync<RegistroCalificadoCompleto>();
        if (maestro == null)
        {
            return null;
        }

        maestro.Enfoques = (await lector.ReadAsync<EnfoqueDelRegistro>()).ToList();
        maestro.Actividades = (await lector.ReadAsync<ActividadDelRegistro>()).ToList();
        return maestro;
    }
}
