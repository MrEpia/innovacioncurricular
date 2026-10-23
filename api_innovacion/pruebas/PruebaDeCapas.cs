namespace ApiInnovacion.Pruebas;

/// <summary>
/// La PRUEBA DE CAPAS: los servicios reales contra repositorios de mentiras,
/// SIN base de datos (Artículo 3).
///
/// Cada bloque escribe sus comprobaciones en su propio archivo
/// (PruebasV1, PruebasAcademico, ...) y aquí solo se llaman: así los tres
/// estudiantes no editan el mismo archivo en la misma ronda.
///
/// Se llama PruebaDeCapas y no Programa: desde la v2 existe el modelo
/// Programa (la tabla programa), y dentro de este proyecto el nombre corto
/// resolvería a la clase equivocada.
/// </summary>
public class PruebaDeCapas
{
    public static async Task<int> Main(string[] args)
    {
        Console.WriteLine("=== Prueba de capas — SIN base de datos ===");

        await PruebasV1.Ejecutar();
        await PruebasAcademico.Ejecutar();
        await PruebasActividades.Ejecutar();
        await PruebasPuentes.Ejecutar();

        Console.WriteLine();
        Console.WriteLine(Verificador.TodoBien
            ? $"=== Prueba de capas completada CON ÉXITO ({Verificador.Comprobaciones} comprobaciones) ==="
            : "=== Prueba de capas FALLIDA: mire los [ERROR] de arriba ===");

        // El código de salida es lo que mira quien corre la prueba en
        // automático: 0 = pasó, cualquier otra cosa = falló.
        return Verificador.TodoBien ? 0 : 1;
    }
}
