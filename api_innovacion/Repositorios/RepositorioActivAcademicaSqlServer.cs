using System.Data;
using ApiInnovacion.Excepciones;
using ApiInnovacion.Modelos;
using Dapper;
using Microsoft.Data.SqlClient;

namespace ApiInnovacion.Repositorios;

/// <summary>
/// La capa 3 contra SQL Server, con Dapper, para activ_academica. El SQL se escribe
/// a mano, queda a la vista y SIEMPRE va parametrizado (@parametro).
///
/// Todas las consultas filtran por activo = 1: el borrado es lógico.
/// </summary>
public class RepositorioActivAcademicaSqlServer : IRepositorioActivAcademica
{
    private readonly string _cadenaConexion;

    public RepositorioActivAcademicaSqlServer(IConfiguration configuracion)
    {
        _cadenaConexion = configuracion.GetConnectionString("SqlServer")
            ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'SqlServer'.");
    }

    private IDbConnection CrearConexion() => new SqlConnection(_cadenaConexion);

    // Los alias traducen snake_case (tabla) a PascalCase (entidad): Dapper
    // mapea por nombre.
    private const string COLUMNAS = @"id AS Id, nombre AS Nombre, num_creditos AS NumCreditos, tipo AS Tipo, area_formacion AS AreaFormacion, h_acom AS HAcom, h_indep AS HIndep, idioma AS Idioma, espejo AS Espejo, entidad_espejo AS EntidadEspejo, pais_espejo AS PaisEspejo, disenio AS Disenio";

    public async Task<IEnumerable<ActivAcademica>> ObtenerTodos(int limite)
    {
        using var conexion = CrearConexion();
        var sql = $@"
            SELECT TOP (@Limite) {COLUMNAS}
            FROM activ_academica
            WHERE activo = 1
            ORDER BY id ASC";

        return await conexion.QueryAsync<ActivAcademica>(sql, new { Limite = limite });
    }

    public async Task<ActivAcademica?> ObtenerPorId(int id)
    {
        using var conexion = CrearConexion();
        // Un registro inactivo responde como inexistente.
        var sql = $@"
            SELECT {COLUMNAS}
            FROM activ_academica
            WHERE id = @Id AND activo = 1";

        return await conexion.QueryFirstOrDefaultAsync<ActivAcademica>(sql, new { Id = id });
    }

    public async Task Crear(ActivAcademica entidad)
    {
        using var conexion = CrearConexion();
        const string sql = @"
            INSERT INTO activ_academica (id, nombre, num_creditos, tipo, area_formacion, h_acom, h_indep, idioma, espejo, entidad_espejo, pais_espejo, disenio, activo)
            VALUES (@Id, @Nombre, @NumCreditos, @Tipo, @AreaFormacion, @HAcom, @HIndep, @Idioma, @Espejo, @EntidadEspejo, @PaisEspejo, @Disenio, 1)";

        try
        {
            await conexion.ExecuteAsync(sql, entidad);
        }
        catch (SqlException ex) when (ex.Number is 2627 or 2601)
        {
            // 2627/2601: violación de llave primaria o índice único. Se traduce
            // aquí para que el servicio nunca vea un tipo de SqlClient.
            throw new LlaveDuplicadaExcepcion(
                "Ya existe un registro de activ_academica con esa llave (puede estar retirado).");
        }
    }

    public async Task<int> Reemplazar(ActivAcademica entidad)
    {
        using var conexion = CrearConexion();
        const string sql = @"
            UPDATE activ_academica
            SET nombre = @Nombre, num_creditos = @NumCreditos, tipo = @Tipo, area_formacion = @AreaFormacion, h_acom = @HAcom, h_indep = @HIndep, idioma = @Idioma, espejo = @Espejo, entidad_espejo = @EntidadEspejo, pais_espejo = @PaisEspejo, disenio = @Disenio
            WHERE id = @Id AND activo = 1";

        return await conexion.ExecuteAsync(sql, entidad);
    }

    public async Task<int> ActualizarParcial(int id, ActivAcademicaCambios cambios)
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
        if (cambios.NumCreditos != null)
        {
            asignaciones.Add("num_creditos = @NumCreditos");
            parametros.Add("NumCreditos", cambios.NumCreditos);
        }
        if (cambios.Tipo != null)
        {
            asignaciones.Add("tipo = @Tipo");
            parametros.Add("Tipo", cambios.Tipo);
        }
        if (cambios.AreaFormacion != null)
        {
            asignaciones.Add("area_formacion = @AreaFormacion");
            parametros.Add("AreaFormacion", cambios.AreaFormacion);
        }
        if (cambios.HAcom != null)
        {
            asignaciones.Add("h_acom = @HAcom");
            parametros.Add("HAcom", cambios.HAcom);
        }
        if (cambios.HIndep != null)
        {
            asignaciones.Add("h_indep = @HIndep");
            parametros.Add("HIndep", cambios.HIndep);
        }
        if (cambios.Idioma != null)
        {
            asignaciones.Add("idioma = @Idioma");
            parametros.Add("Idioma", cambios.Idioma);
        }
        if (cambios.Espejo != null)
        {
            asignaciones.Add("espejo = @Espejo");
            parametros.Add("Espejo", cambios.Espejo);
        }
        if (cambios.EntidadEspejo != null)
        {
            asignaciones.Add("entidad_espejo = @EntidadEspejo");
            parametros.Add("EntidadEspejo", cambios.EntidadEspejo);
        }
        if (cambios.PaisEspejo != null)
        {
            asignaciones.Add("pais_espejo = @PaisEspejo");
            parametros.Add("PaisEspejo", cambios.PaisEspejo);
        }
        if (cambios.Disenio != null)
        {
            asignaciones.Add("disenio = @Disenio");
            parametros.Add("Disenio", cambios.Disenio);
        }

        if (asignaciones.Count == 0) return 0;

        var sql = $"UPDATE activ_academica SET {string.Join(", ", asignaciones)} WHERE id = @Id AND activo = 1";

        return await conexion.ExecuteAsync(sql, parametros);
    }

    public async Task<int> EliminarLogico(int id)
    {
        using var conexion = CrearConexion();
        // Borrado LÓGICO en una sola consulta: cero filas = no existe o ya
        // estaba inactivo, que es el 404 del contrato.
        const string sql = @"
            UPDATE activ_academica
            SET activo = 0
            WHERE id = @Id AND activo = 1";

        return await conexion.ExecuteAsync(sql, new { Id = id });
    }
}
