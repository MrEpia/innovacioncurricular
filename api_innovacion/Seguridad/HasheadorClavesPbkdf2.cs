using System.Security.Cryptography;

namespace ApiInnovacion.Seguridad;

/// <summary>
/// PBKDF2 con SHA-256, que trae .NET (System.Security.Cryptography): no hace
/// falta un paquete externo como BCrypt (decisión D-v3-2).
///
/// Lo que se guarda en usuario.password NO es la contraseña: es
///   pbkdf2-sha256$100000$&lt;sal en base64&gt;$&lt;hash en base64&gt;
///
///   - La SAL es aleatoria y distinta para cada usuario: dos personas con la
///     misma contraseña tienen hashes distintos, y una tabla precalculada no
///     sirve para atacarlas a todas a la vez.
///   - Las 100 000 ITERACIONES hacen que probar una contraseña cueste tiempo:
///     despreciable para un ingreso, enorme para quien prueba millones.
///   - El número de iteraciones viaja dentro del hash: si mañana se sube, los
///     hashes viejos se siguen pudiendo verificar.
/// </summary>
public class HasheadorClavesPbkdf2 : IHasheadorClaves
{
    private const string Prefijo = "pbkdf2-sha256";
    private const int Iteraciones = 100_000;
    private const int BytesSal = 16;
    private const int BytesHash = 32;

    public string Hashear(string clave)
    {
        var sal = RandomNumberGenerator.GetBytes(BytesSal);
        var hash = Rfc2898DeriveBytes.Pbkdf2(clave, sal, Iteraciones, HashAlgorithmName.SHA256, BytesHash);
        return $"{Prefijo}${Iteraciones}${Convert.ToBase64String(sal)}${Convert.ToBase64String(hash)}";
    }

    public bool Verificar(string clave, string hashGuardado)
    {
        var partes = hashGuardado.Split('$');
        if (partes.Length != 4 || partes[0] != Prefijo || !int.TryParse(partes[1], out var iteraciones))
        {
            // Un valor que no tiene este formato —por ejemplo, una contraseña
            // guardada en texto plano— NUNCA se acepta.
            return false;
        }

        byte[] sal, esperado;
        try
        {
            sal = Convert.FromBase64String(partes[2]);
            esperado = Convert.FromBase64String(partes[3]);
        }
        catch (FormatException)
        {
            return false;
        }

        var calculado = Rfc2898DeriveBytes.Pbkdf2(clave, sal, iteraciones, HashAlgorithmName.SHA256, esperado.Length);

        // Comparación en TIEMPO CONSTANTE: un == normal se detiene en el primer
        // byte distinto, y medir cuánto tarda revela cuántos bytes acertó el
        // atacante.
        return CryptographicOperations.FixedTimeEquals(calculado, esperado);
    }
}
