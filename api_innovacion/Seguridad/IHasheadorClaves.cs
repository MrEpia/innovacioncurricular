namespace ApiInnovacion.Seguridad;

/// <summary>Convierte una contraseña en un hash que no se puede revertir, y la verifica.</summary>
public interface IHasheadorClaves
{
    string Hashear(string clave);
    bool Verificar(string clave, string hashGuardado);
}
