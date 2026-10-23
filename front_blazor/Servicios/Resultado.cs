namespace FrontInnovacion.Servicios;

/// <summary>
/// Lo que devuelve cada operación: si salió bien, qué trajo, y qué errores hay
/// que mostrar.
///
/// Existe para que las páginas **no vean códigos de estado**. Una página
/// pregunta «¿salió bien?», no «¿fue 200 o 204?».
///
/// Antes vivía dentro de ServicioAliado.cs; se movió a su propio archivo en la
/// v2 porque ya la usan veintidós servicios, no uno.
/// </summary>
public record Resultado<T>(bool Ok, T? Datos, List<string> Errores)
{
    public static Resultado<T> Bien(T datos) => new(true, datos, new());
    public static Resultado<T> Mal(List<string> errores) => new(false, default, errores);
}
