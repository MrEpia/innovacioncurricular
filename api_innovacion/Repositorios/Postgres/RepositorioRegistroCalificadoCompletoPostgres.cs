// PostgreSQL: consultas explícitas; conserva el contrato del repositorio SQL Server.
using System.Data;
using System.Text.Json;
using ApiInnovacion.Excepciones;
using ApiInnovacion.Modelos;
using Dapper;
using Npgsql;

namespace ApiInnovacion.Repositorios.Postgres;

public class RepositorioRegistroCalificadoCompletoPostgres : IRepositorioRegistroCalificadoCompleto
{
    private readonly string _cadenaConexion;

    public RepositorioRegistroCalificadoCompletoPostgres(ConfiguracionPostgres configuracion)
    {
        _cadenaConexion = configuracion.CadenaConexion;
    }

    private IDbConnection CrearConexion() => new NpgsqlConnection(_cadenaConexion);

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
            await conexion.ExecuteAsync("SELECT * FROM public.sp_registro_calificado_completo_crear(@Codigo, @CantCreditos, @HoraAcom, @HoraInd, @Metodologia, @FechaInicio, @FechaFin, @DuracionAnios, @DuracionSemestres, @TipoTitulacion, @Programa, @Enfoques, @Actividades)", parametros);
        }
        catch (PostgresException ex) when (ex.SqlState == "23505")
        {
            throw new LlaveDuplicadaExcepcion(
                "Ya existe un registro de registro_calificado con esa llave (puede estar retirado). No se guardó nada.");
        }
        catch (PostgresException ex) when (ex.SqlState == "P0400")
        {

            throw new ReferenciaInvalidaExcepcion(ex.MessageText);
        }
    }

    public async Task<RegistroCalificadoCompleto?> ObtenerCompleto(int codigo)
    {
        using var conexion = CrearConexion();
        const string sql = @"
            SELECT codigo AS Codigo, cant_creditos AS CantCreditos, hora_acom AS HoraAcom, hora_ind AS HoraInd,
                   metodologia AS Metodologia, fecha_inicio::timestamp AS FechaInicio, fecha_fin::timestamp AS FechaFin,
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
