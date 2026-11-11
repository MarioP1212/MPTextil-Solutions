using System.Security.Cryptography;
using MPTextil.Application.Abstracciones.Seguridad;

namespace MPTextil.Infrastructure.Seguridad;

internal sealed class HasherContrasenaPbkdf2 : IHasherContrasena
{
    private const string Algoritmo = "PBKDF2-SHA256";
    private const int Iteraciones = 600_000;
    private const int TamanoSal = 16;
    private const int TamanoHash = 32;
    private const int PartesDelHash = 4;
    private const char Separador = '.';

    public string Hashear(string contrasena)
    {
        var sal = RandomNumberGenerator.GetBytes(TamanoSal);
        var hash = Derivar(contrasena, sal, Iteraciones, TamanoHash);

        return string.Join(Separador, Algoritmo, Iteraciones, Convert.ToBase64String(sal), Convert.ToBase64String(hash));
    }

    public bool Verificar(string contrasena, string hashAlmacenado)
    {
        var partes = hashAlmacenado.Split(Separador);

        if (partes.Length != PartesDelHash
            || partes[0] != Algoritmo
            || !int.TryParse(partes[1], out var iteraciones)
            || DesdeBase64(partes[2]) is not { } sal
            || DesdeBase64(partes[3]) is not { } hashEsperado)
        {
            return false;
        }

        var hashCalculado = Derivar(contrasena, sal, iteraciones, hashEsperado.Length);

        return CryptographicOperations.FixedTimeEquals(hashCalculado, hashEsperado);
    }

    private static byte[] Derivar(string contrasena, byte[] sal, int iteraciones, int tamano) =>
        Rfc2898DeriveBytes.Pbkdf2(contrasena, sal, iteraciones, HashAlgorithmName.SHA256, tamano);

    private static byte[]? DesdeBase64(string texto)
    {
        var buffer = new byte[texto.Length];

        return Convert.TryFromBase64String(texto, buffer, out var bytesEscritos) && bytesEscritos > 0
            ? buffer[..bytesEscritos]
            : null;
    }
}
