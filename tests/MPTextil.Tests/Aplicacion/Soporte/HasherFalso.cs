using MPTextil.Application.Abstracciones.Seguridad;

namespace MPTextil.Tests.Aplicacion.Soporte;

internal sealed class HasherFalso : IHasherContrasena
{
    public int HashesCalculados { get; private set; }

    public static string HashDe(string contrasena) => $"hash:{contrasena}";

    public string Hashear(string contrasena)
    {
        HashesCalculados++;
        return HashDe(contrasena);
    }

    public bool Verificar(string contrasena, string hashAlmacenado)
    {
        HashesCalculados++;
        return hashAlmacenado == HashDe(contrasena);
    }
}
