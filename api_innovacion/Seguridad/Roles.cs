namespace ApiInnovacion.Seguridad;

/// <summary>
/// Los tres roles del sistema, tal como están en la tabla rol (los siembra
/// db/migraciones/v3_001_seguridad.sql). Se escriben UNA vez aquí para que un
/// error de tipeo en un [Authorize] no compile en vez de dejar una puerta abierta.
///
/// Qué puede cada uno (2_spec.md de la v3, RF4):
///   consulta       → solo leer (GET)
///   editor         → leer, crear y modificar (GET, POST, PUT, PATCH), pero NO retirar
///   administrador  → todo, incluido retirar y administrar usuarios y roles
/// </summary>
public static class Roles
{
    public const string Administrador = "administrador";
    public const string Editor = "editor";
    public const string Consulta = "consulta";
}
