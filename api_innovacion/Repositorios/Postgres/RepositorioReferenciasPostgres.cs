// PostgreSQL: consultas explícitas; conserva el contrato del repositorio SQL Server.
using System.Data;
using Dapper;
using Npgsql;

namespace ApiInnovacion.Repositorios.Postgres;

public class RepositorioReferenciasPostgres : IRepositorioReferencias
{
    private readonly string _cadenaConexion;

    public RepositorioReferenciasPostgres(ConfiguracionPostgres configuracion)
    {
        _cadenaConexion = configuracion.CadenaConexion;
    }

    private IDbConnection CrearConexion() => new NpgsqlConnection(_cadenaConexion);

    private async Task<bool> Existe(string sql, object parametros)
    {
        using var conexion = CrearConexion();
        return await conexion.ExecuteScalarAsync<int>(sql, parametros) > 0;
    }

    private sealed class Conteo
    {
        public string Tabla { get; set; } = string.Empty;
        public int Cantidad { get; set; }
    }

    private async Task<IReadOnlyList<string>> Dependientes(string procedimiento, object parametros)
    {
        using var conexion = CrearConexion();
        var conteos = await conexion.QueryAsync<Conteo>(
            procedimiento, parametros);
        return conteos
            .Where(c => c.Cantidad > 0)
            .Select(c => $"{c.Tabla} ({c.Cantidad})")
            .ToList();
    }

    public Task<bool> UniversidadActiva(int id) =>
        Existe("SELECT COUNT(*) FROM universidad WHERE id = @Id AND activo = 1", new { Id = id });

    public Task<bool> FacultadActiva(int id) =>
        Existe("SELECT COUNT(*) FROM facultad WHERE id = @Id AND activo = 1", new { Id = id });

    public Task<bool> ProgramaActivo(int id) =>
        Existe("SELECT COUNT(*) FROM programa WHERE id = @Id AND activo = 1", new { Id = id });

    public Task<bool> RegistroCalificadoActivo(int codigo) =>
        Existe("SELECT COUNT(*) FROM registro_calificado WHERE codigo = @Codigo AND activo = 1", new { Codigo = codigo });

    public Task<bool> ActivAcademicaActiva(int id) =>
        Existe("SELECT COUNT(*) FROM activ_academica WHERE id = @Id AND activo = 1", new { Id = id });

    public Task<bool> AreaConocimientoActiva(string id) =>
        Existe("SELECT COUNT(*) FROM area_conocimiento WHERE id = @Id AND activo = 1", new { Id = id });

    public Task<bool> PracticaEstrategiaActiva(int id) =>
        Existe("SELECT COUNT(*) FROM practica_estrategia WHERE id = @Id AND activo = 1", new { Id = id });

    public Task<bool> CarInnovacionActiva(int id) =>
        Existe("SELECT COUNT(*) FROM car_innovacion WHERE id = @Id AND activo = 1", new { Id = id });

    public Task<bool> AspectoNormativoActivo(int id) =>
        Existe("SELECT COUNT(*) FROM aspecto_normativo WHERE id = @Id AND activo = 1", new { Id = id });

    public Task<bool> EnfoqueActivo(int id) =>
        Existe("SELECT COUNT(*) FROM enfoque WHERE id = @Id AND activo = 1", new { Id = id });

    public Task<bool> AliadoActivo(int nit) =>
        Existe("SELECT COUNT(*) FROM aliado WHERE nit = @Nit AND activo = 1", new { Nit = nit });

    public Task<bool> UsuarioActivo(int id) =>
        Existe("SELECT COUNT(*) FROM usuario WHERE id = @Id AND activo = 1", new { Id = id });

    public Task<bool> RolActivo(int id) =>
        Existe("SELECT COUNT(*) FROM rol WHERE id = @Id AND activo = 1", new { Id = id });

    public Task<IReadOnlyList<string>> DependientesDeUniversidad(int id) =>
        Dependientes("SELECT * FROM public.sp_dependientes_universidad(@Id)", new { Id = id });

    public Task<IReadOnlyList<string>> DependientesDeFacultad(int id) =>
        Dependientes("SELECT * FROM public.sp_dependientes_facultad(@Id)", new { Id = id });

    public Task<IReadOnlyList<string>> DependientesDePrograma(int id) =>
        Dependientes("SELECT * FROM public.sp_dependientes_programa(@Id)", new { Id = id });

    public Task<IReadOnlyList<string>> DependientesDeRegistroCalificado(int codigo) =>
        Dependientes("SELECT * FROM public.sp_dependientes_registro_calificado(@Codigo)", new { Codigo = codigo });

    public Task<IReadOnlyList<string>> DependientesDeActivAcademica(int id) =>
        Dependientes("SELECT * FROM public.sp_dependientes_activ_academica(@Id)", new { Id = id });

    public Task<IReadOnlyList<string>> DependientesDeAreaConocimiento(string id) =>
        Dependientes("SELECT * FROM public.sp_dependientes_area_conocimiento(@Id)", new { Id = id });

    public Task<IReadOnlyList<string>> DependientesDePracticaEstrategia(int id) =>
        Dependientes("SELECT * FROM public.sp_dependientes_practica_estrategia(@Id)", new { Id = id });

    public Task<IReadOnlyList<string>> DependientesDeCarInnovacion(int id) =>
        Dependientes("SELECT * FROM public.sp_dependientes_car_innovacion(@Id)", new { Id = id });

    public Task<IReadOnlyList<string>> DependientesDeAspectoNormativo(int id) =>
        Dependientes("SELECT * FROM public.sp_dependientes_aspecto_normativo(@Id)", new { Id = id });

    public Task<IReadOnlyList<string>> DependientesDeEnfoque(int id) =>
        Dependientes("SELECT * FROM public.sp_dependientes_enfoque(@Id)", new { Id = id });

    public Task<IReadOnlyList<string>> DependientesDeAliado(int nit) =>
        Dependientes("SELECT * FROM public.sp_dependientes_aliado(@Nit)", new { Nit = nit });
}
