using MPTextil.Infrastructure.Seguridad;

namespace MPTextil.Tests.Infraestructura;

public sealed class HasherContrasenaPbkdf2Tests
{
    private const string Contrasena = "MPTextil2026!";

    private readonly HasherContrasenaPbkdf2 _hasher = new();

    [Fact]
    public void Verificar_ConLaMismaContrasena_DevuelveVerdadero()
    {
        var hash = _hasher.Hashear(Contrasena);

        Assert.True(_hasher.Verificar(Contrasena, hash));
    }

    [Fact]
    public void Verificar_ConOtraContrasena_DevuelveFalso()
    {
        var hash = _hasher.Hashear(Contrasena);

        Assert.False(_hasher.Verificar("mptextil2026!", hash));
    }

    [Fact]
    public void Hashear_LaMismaContrasenaDosVeces_GeneraHashesDistintos()
    {
        var primero = _hasher.Hashear(Contrasena);
        var segundo = _hasher.Hashear(Contrasena);

        Assert.NotEqual(primero, segundo);
    }

    [Fact]
    public void Hashear_GuardaAlgoritmoIteracionesSalYHash()
    {
        var partes = _hasher.Hashear(Contrasena).Split('.');

        Assert.Equal(4, partes.Length);
        Assert.Equal("PBKDF2-SHA256", partes[0]);
        Assert.Equal("600000", partes[1]);
        Assert.DoesNotContain(Contrasena, partes);
    }

    [Theory]
    [InlineData("")]
    [InlineData("texto-sin-formato")]
    [InlineData("MD5.1000.AAAAAAAAAAAAAAAAAAAAAA==.AAAAAAAAAAAAAAAAAAAAAA==")]
    [InlineData("PBKDF2-SHA256.mil.AAAAAAAAAAAAAAAAAAAAAA==.AAAAAAAAAAAAAAAAAAAAAA==")]
    [InlineData("PBKDF2-SHA256.1000.@@@.AAAAAAAAAAAAAAAAAAAAAA==")]
    public void Verificar_ConHashMalFormado_DevuelveFalso(string hashAlmacenado)
    {
        Assert.False(_hasher.Verificar(Contrasena, hashAlmacenado));
    }
}
