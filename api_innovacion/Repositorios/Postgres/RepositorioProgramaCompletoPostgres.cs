// PostgreSQL: consultas explícitas; conserva el contrato del repositorio SQL Server.
using System.Data;
using System.Text.Json;
using ApiInnovacion.Excepciones;
using ApiInnovacion.Modelos;
using Dapper;
using Npgsql;

namespace ApiInnovacion.Repositorios.Postgres;

public class RepositorioProgramaCompletoPostgres : IRepositorioProgramaCompleto
{
    private readonly string _cadenaConexion;

    public RepositorioProgramaCompletoPostgres(ConfiguracionPostgres configuracion)
    {
        _cadenaConexion = configuracion.CadenaConexion;
    }

    private IDbConnection CrearConexion() => new NpgsqlConnection(_cadenaConexion);

    public async Task Crear(ProgramaCompleto entidad)
    {
        using var conexion = CrearConexion();
        var parametros = new
        {
            entidad.Id,
            entidad.Nombre,
            entidad.Tipo,
            entidad.Nivel,
            entidad.FechaCreacion,
            entidad.FechaCierre,
            entidad.NumeroCohortes,
            entidad.CantGraduados,
            entidad.FechaActualizacion,
            entidad.Ciudad,
            entidad.Facultad,
            Areas = JsonSerializer.Serialize(entidad.AreasConocimiento.Select(a => a.AreaConocimiento)),
            Practicas = JsonSerializer.Serialize(entidad.PracticasEstrategias.Select(p => p.PracticaEstrategia)),
            Caracteristicas = JsonSerializer.Serialize(entidad.CaracteristicasInnovacion.Select(c => c.CarInnovacion))
        };

        try
        {
            await conexion.ExecuteAsync("SELECT * FROM public.sp_programa_completo_crear(@Id, @Nombre, @Tipo, @Nivel, @FechaCreacion, @FechaCierre, @NumeroCohortes, @CantGraduados, @FechaActualizacion, @Ciudad, @Facultad, @Areas, @Practicas, @Caracteristicas)", parametros);
        }
        catch (PostgresException ex) when (ex.SqlState == "23505")
        {
            throw new LlaveDuplicadaExcepcion(
                "Ya existe un registro de programa con esa llave (puede estar retirado). No se guardó nada.");
        }
        catch (PostgresException ex) when (ex.SqlState == "P0400")
        {
            throw new ReferenciaInvalidaExcepcion(ex.MessageText);
        }
    }

    public async Task<ProgramaCompleto?> ObtenerCompleto(int id)
    {
        using var conexion = CrearConexion();
        const string sql = @"
            SELECT id AS Id, nombre AS Nombre, tipo AS Tipo, nivel AS Nivel, fecha_creacion AS FechaCreacion,
                   fecha_cierre AS FechaCierre, numero_cohortes AS NumeroCohortes, cant_graduados AS CantGraduados,
                   fecha_actualizacion AS FechaActualizacion, ciudad AS Ciudad, facultad AS Facultad
            FROM programa
            WHERE id = @Id AND activo = 1;

            SELECT pa.area_conocimiento AS AreaConocimiento, a.area AS Nombre
            FROM programa_ac pa
            JOIN area_conocimiento a ON a.id = pa.area_conocimiento
            WHERE pa.programa = @Id AND pa.activo = 1
            ORDER BY pa.area_conocimiento;

            SELECT pp.practica_estrategia AS PracticaEstrategia, p.nombre AS Nombre
            FROM programa_pe pp
            JOIN practica_estrategia p ON p.id = pp.practica_estrategia
            WHERE pp.programa = @Id AND pp.activo = 1
            ORDER BY pp.practica_estrategia;

            SELECT pc.car_innovacion AS CarInnovacion, c.nombre AS Nombre
            FROM programa_ci pc
            JOIN car_innovacion c ON c.id = pc.car_innovacion
            WHERE pc.programa = @Id AND pc.activo = 1
            ORDER BY pc.car_innovacion;";

        using var lector = await conexion.QueryMultipleAsync(sql, new { Id = id });
        var maestro = await lector.ReadFirstOrDefaultAsync<ProgramaCompleto>();
        if (maestro == null)
        {
            return null;
        }

        maestro.AreasConocimiento = (await lector.ReadAsync<AreaDelPrograma>()).ToList();
        maestro.PracticasEstrategias = (await lector.ReadAsync<PracticaDelPrograma>()).ToList();
        maestro.CaracteristicasInnovacion = (await lector.ReadAsync<CaracteristicaDelPrograma>()).ToList();
        return maestro;
    }
}
