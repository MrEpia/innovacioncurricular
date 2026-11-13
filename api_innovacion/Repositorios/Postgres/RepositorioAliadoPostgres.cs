// PostgreSQL: consultas explícitas; conserva el contrato del repositorio SQL Server.
using ApiInnovacion.Excepciones;
using System.Data;
using ApiInnovacion.Modelos;
using Dapper;
using Npgsql;

namespace ApiInnovacion.Repositorios.Postgres;

public class RepositorioAliadoPostgres : IRepositorioAliado
{
    private readonly string _cadenaConexion;

    public RepositorioAliadoPostgres(ConfiguracionPostgres configuracion)
    {
        _cadenaConexion = configuracion.CadenaConexion;
    }

    private IDbConnection CrearConexion() => new NpgsqlConnection(_cadenaConexion);

    private const string COLUMNAS = @"nit AS Nit, razon_social AS RazonSocial,
            nombre_contacto AS NombreContacto, correo AS Correo,
            telefono AS Telefono, ciudad AS Ciudad";

    public async Task<IEnumerable<Aliado>> ObtenerTodos(int limite)
    {
        using var conexion = CrearConexion();
        var sql = $@"
            SELECT {COLUMNAS}
            FROM aliado
            WHERE activo = 1
            ORDER BY nit ASC LIMIT @Limite";

        return await conexion.QueryAsync<Aliado>(sql, new { Limite = limite });
    }

    public async Task<Aliado?> ObtenerPorNit(int nit)
    {
        using var conexion = CrearConexion();

        var sql = $@"
            SELECT {COLUMNAS}
            FROM aliado
            WHERE nit = @Nit AND activo = 1";

        return await conexion.QueryFirstOrDefaultAsync<Aliado>(sql, new { Nit = nit });
    }

    public async Task Crear(Aliado aliado)
    {
        using var conexion = CrearConexion();

        const string sql = @"
            INSERT INTO aliado (nit, razon_social, nombre_contacto, correo, telefono, ciudad, activo)
            VALUES (@Nit, @RazonSocial, @NombreContacto, @Correo, @Telefono, @Ciudad, 1)";

        await conexion.ExecuteAsync(sql, aliado);
    }

    public async Task<int> Reemplazar(Aliado aliado)
    {
        using var conexion = CrearConexion();
        const string sql = @"
            UPDATE aliado
            SET razon_social = @RazonSocial, nombre_contacto = @NombreContacto,
                correo = @Correo, telefono = @Telefono, ciudad = @Ciudad
            WHERE nit = @Nit AND activo = 1";

        return await conexion.ExecuteAsync(sql, aliado);
    }

    public async Task<int> ActualizarParcial(int nit, string? razonSocial, string? nombreContacto,
                                             string? correo, string? telefono, string? ciudad)
    {
        using var conexion = CrearConexion();

        var asignaciones = new List<string>();
        var parametros = new DynamicParameters();
        parametros.Add("Nit", nit);

        if (razonSocial != null)
        {
            asignaciones.Add("razon_social = @RazonSocial");
            parametros.Add("RazonSocial", razonSocial);
        }
        if (nombreContacto != null)
        {
            asignaciones.Add("nombre_contacto = @NombreContacto");
            parametros.Add("NombreContacto", nombreContacto);
        }
        if (correo != null)
        {
            asignaciones.Add("correo = @Correo");
            parametros.Add("Correo", correo);
        }
        if (telefono != null)
        {
            asignaciones.Add("telefono = @Telefono");
            parametros.Add("Telefono", telefono);
        }
        if (ciudad != null)
        {
            asignaciones.Add("ciudad = @Ciudad");
            parametros.Add("Ciudad", ciudad);
        }

        if (asignaciones.Count == 0) return 0;

        var sql = $"UPDATE aliado SET {string.Join(", ", asignaciones)} WHERE nit = @Nit AND activo = 1";

        return await conexion.ExecuteAsync(sql, parametros);
    }

    public async Task<int> EliminarLogico(int nit)
    {
        using var conexion = CrearConexion();

        try
        {
            return await conexion.ExecuteScalarAsync<int>(
                "SELECT * FROM public.sp_retirar_aliado(@Nit)", new { Nit = nit });
        }
        catch (PostgresException ex) when (ex.SqlState == "P0409")
        {
            throw new DependenciasActivasExcepcion(
                $"No se puede retirar el aliado {nit}: {ex.MessageText}. Retírelos primero.");
        }
    }
}
