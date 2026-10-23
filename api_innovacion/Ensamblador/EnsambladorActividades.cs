namespace ApiInnovacion.Ensamblador;

/// <summary>
/// El ensamblador del bloque «Actividades» de la v2: activ_academica, pasantia, premio, docente_departamento y alianza.
///
/// Responsable: David Epia. Este archivo lo llena SOLO su responsable, en su rama:
/// así los tres bloques de la v2 se integran sin tocarse (enmienda 1.1.0 del
/// Artículo 3).
/// </summary>
public static class EnsambladorActividades
{
    public static IServiceCollection AgregarBloqueActividades(this IServiceCollection servicios)
    {
        return servicios;
    }
}
