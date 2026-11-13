using Npgsql;

namespace ApiInnovacion.Repositorios.Postgres;

/// <summary>Valida sin registrar credenciales. Neon requiere TLS y verificación de hostname.</summary>
public sealed class ConfiguracionPostgres
{
    public string CadenaConexion { get; }

    public ConfiguracionPostgres(IConfiguration configuracion)
    {
        var cadena = configuracion.GetConnectionString("Postgres");
        if (string.IsNullOrWhiteSpace(cadena))
            throw new InvalidOperationException("Falta ConnectionStrings__Postgres (formato Npgsql).");
        NpgsqlConnectionStringBuilder opciones;
        try { opciones = new NpgsqlConnectionStringBuilder(cadena); }
        catch { throw new InvalidOperationException("ConnectionStrings__Postgres tiene un formato inválido; use formato Npgsql."); }
        var entorno = configuracion["ASPNETCORE_ENVIRONMENT"] ?? "Production";
        if (!entorno.Equals("Development", StringComparison.OrdinalIgnoreCase) && opciones.SslMode != SslMode.VerifyFull)
            throw new InvalidOperationException("PostgreSQL en producción requiere SSL Mode=VerifyFull.");
        opciones.IncludeErrorDetail = false;
        opciones.PersistSecurityInfo = false;
        // Neon usa TLS; no intentar localizar credenciales Kerberos en el runtime mínimo.
        opciones["GSS Encryption Mode"] = "Disable";
        CadenaConexion = opciones.ConnectionString;
    }
}
