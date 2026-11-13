using System.Security.Cryptography;
using System.Text;
using ApiInnovacion.Repositorios.Postgres;
using Npgsql;

namespace ApiInnovacion.Infraestructura;

/// <summary>Historial con checksum, bloqueo de sesión y transacción por migración.</summary>
public static class MigradorPostgres
{
    public static async Task Ejecutar(IConfiguration configuracion, bool incluirDemo = false)
    {
        var opciones = new ConfiguracionPostgres(configuracion);
        await using var conexion = new NpgsqlConnection(opciones.CadenaConexion);
        await conexion.OpenAsync();
        await using var bloqueo = new NpgsqlCommand("SELECT pg_advisory_lock(7413142026)", conexion);
        await bloqueo.ExecuteNonQueryAsync();
        try
        {
            await using var comprobar = new NpgsqlCommand("""
                SELECT EXISTS(SELECT 1 FROM pg_tables WHERE schemaname='public')
                   AND to_regclass('public.astra_migraciones') IS NULL
                """, conexion);
            if ((bool)(await comprobar.ExecuteScalarAsync())!)
                throw new InvalidOperationException("Base existente sin historial ASTRA: se rechaza inicializarla. Use una base nueva y separada.");
            await using var historial = new NpgsqlCommand("""
                CREATE TABLE IF NOT EXISTS public.astra_migraciones (
                    version text PRIMARY KEY, checksum text NOT NULL, aplicada_en timestamptz NOT NULL DEFAULT now())
                """, conexion);
            await historial.ExecuteNonQueryAsync();
            var directorio = Path.Combine(AppContext.BaseDirectory, "MigracionesPostgres");
            var archivos = Directory.GetFiles(directorio, "*.sql").AsEnumerable();
            if (incluirDemo) archivos = archivos.Concat(Directory.GetFiles(Path.Combine(directorio, "demo"), "*.sql"));
            var ordenados = archivos.OrderBy(x => Path.GetFileName(x), StringComparer.Ordinal).ToArray();
            if (ordenados.Length == 0) throw new InvalidOperationException("No se incluyeron las migraciones PostgreSQL en la publicación.");
            foreach (var archivo in ordenados)
            {
                var version = Path.GetFileName(archivo);
                // Git conserva SQL en LF; Windows puede construir imágenes desde CRLF.
                // El contenido lógico tiene el mismo checksum en ambos entornos.
                var sql = (await File.ReadAllTextAsync(archivo, Encoding.UTF8)).Replace("\r\n", "\n").Replace("\r", "\n");
                var checksum = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(sql)));
                var checksumWindows = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(sql.Replace("\n", "\r\n"))));
                await using var consultar = new NpgsqlCommand("SELECT checksum FROM public.astra_migraciones WHERE version=@version", conexion);
                consultar.Parameters.AddWithValue("version", version);
                var guardado = await consultar.ExecuteScalarAsync();
                if (guardado is string anterior)
                {
                    if (anterior != checksum && anterior != checksumWindows) throw new InvalidOperationException($"La migración {version} ya aplicada cambió. Cree otra migración; no sobrescriba el historial.");
                    Console.WriteLine($"Omitida (ya aplicada): {version}");
                    continue;
                }
                await using var transaccion = await conexion.BeginTransactionAsync();
                await using var aplicar = new NpgsqlCommand(sql, conexion, transaccion) { CommandTimeout = 120 };
                await aplicar.ExecuteNonQueryAsync();
                await using var registrar = new NpgsqlCommand("INSERT INTO public.astra_migraciones(version,checksum) VALUES(@version,@checksum)", conexion, transaccion);
                registrar.Parameters.AddWithValue("version", version);
                registrar.Parameters.AddWithValue("checksum", checksum);
                await registrar.ExecuteNonQueryAsync();
                await transaccion.CommitAsync();
                Console.WriteLine($"Aplicada: {version}");
            }
        }
        finally
        {
            await using var liberar = new NpgsqlCommand("SELECT pg_advisory_unlock(7413142026)", conexion);
            await liberar.ExecuteNonQueryAsync();
        }
    }
}
