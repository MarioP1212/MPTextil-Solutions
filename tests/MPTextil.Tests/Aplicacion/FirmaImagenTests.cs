using MPTextil.Application.Solicitudes.Acciones;

namespace MPTextil.Tests.Aplicacion;

public sealed class FirmaImagenTests
{
    private static readonly byte[] Jpeg = [0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46, 0x00, 0x01];
    private static readonly byte[] Png = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00, 0x00, 0x0D];
    private static readonly byte[] Webp = [.. "RIFF"u8, 0x24, 0x00, 0x00, 0x00, .. "WEBP"u8];
    private static readonly byte[] Ejecutable = [0x4D, 0x5A, 0x90, 0x00, 0x03, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00];

    [Fact]
    public void Coincide_ConLaFirmaDeCadaFormato_DevuelveVerdadero()
    {
        Assert.True(FirmaImagen.Coincide(Jpeg, "image/jpeg"));
        Assert.True(FirmaImagen.Coincide(Png, "IMAGE/PNG"));
        Assert.True(FirmaImagen.Coincide(Webp, "image/webp"));
    }

    [Fact]
    public void Coincide_EjecutableDisfrazadoDeImagen_DevuelveFalso()
    {
        Assert.False(FirmaImagen.Coincide(Ejecutable, "image/jpeg"));
    }

    [Fact]
    public void Coincide_ContenidoDeOtroFormato_DevuelveFalso()
    {
        Assert.False(FirmaImagen.Coincide(Png, "image/jpeg"));
    }

    [Theory]
    [InlineData("image/gif")]
    [InlineData("application/pdf")]
    [InlineData(null)]
    public void Coincide_TipoNoPermitido_DevuelveFalso(string? contentType)
    {
        Assert.False(FirmaImagen.Coincide(Png, contentType));
    }

    [Fact]
    public void Coincide_ArchivoMasCortoQueLaFirma_DevuelveFalso()
    {
        Assert.False(FirmaImagen.Coincide(Png.AsSpan(0, 4), "image/png"));
        Assert.False(FirmaImagen.Coincide(Webp.AsSpan(0, 10), "image/webp"));
    }
}
