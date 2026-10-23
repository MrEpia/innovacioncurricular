namespace ApiInnovacion.Pruebas;

/// <summary>
/// Las dos maneras de comprobar algo en la prueba de capas, compartidas por
/// todos los bloques. Es un «assert» escrito a mano: lo mismo que hace
/// Assert.Equal de xUnit, sin instalar el framework (Artículo 2).
/// </summary>
public static class Verificador
{
    /// <summary>Si alguna comprobación falló. Lo lee Programa al terminar.</summary>
    public static bool TodoBien { get; private set; } = true;

    public static int Comprobaciones { get; private set; }

    /// <summary>La línea que PUEDE FALLAR: comprueba, narra y deja constancia.</summary>
    public static void Revisar(bool condicion, string ok, string error)
    {
        Comprobaciones++;
        Console.WriteLine(condicion ? $"[OK] {ok}" : $"[ERROR] {error}");
        if (!condicion) TodoBien = false;
    }

    /// <summary>
    /// Comprueba que la operación lance EXACTAMENTE la excepción esperada. Una
    /// excepción de otro tipo también es un fallo: un 400 donde iba un 404 es
    /// un contrato roto.
    /// </summary>
    public static async Task EsperarExcepcion<T>(Func<Task> operacion, string descripcion)
        where T : Exception
    {
        try
        {
            await operacion();
            Revisar(false, "", $"{descripcion}: debió lanzar {typeof(T).Name} y no lanzó nada.");
        }
        catch (T)
        {
            Revisar(true, descripcion, "");
        }
        catch (Exception otra)
        {
            Revisar(false, "", $"{descripcion}: esperaba {typeof(T).Name} y llegó {otra.GetType().Name}.");
        }
    }
}
