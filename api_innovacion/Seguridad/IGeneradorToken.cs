namespace ApiInnovacion.Seguridad;

/// <summary>Un token emitido: el texto que viaja en Authorization y cuándo vence.</summary>
public record TokenEmitido(string Token, DateTime ExpiraUtc);

/// <summary>Fabrica el JWT de un usuario ya autenticado.</summary>
public interface IGeneradorToken
{
    TokenEmitido Generar(int usuarioId, string username, string? nombreCompleto, IEnumerable<string> roles);
}
