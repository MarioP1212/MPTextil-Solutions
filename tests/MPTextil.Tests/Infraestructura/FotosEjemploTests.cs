using MPTextil.Application.Solicitudes.Acciones;
using MPTextil.Domain.Entidades;
using MPTextil.Infrastructure.Persistencia.DatosEjemplo;

namespace MPTextil.Tests.Infraestructura;

public sealed class FotosEjemploTests
{
    [Theory]
    [InlineData("tela")]
    [InlineData("malla")]
    [InlineData("etiqueta")]
    [InlineData("hilo")]
    public void Abrir_CadaMuestra_EsUnPngValidoDentroDelLimite(string muestra)
    {
        using var contenido = FotosEjemplo.Abrir(muestra);
        var cabecera = new byte[FirmaImagen.BytesNecesarios];
        contenido.ReadExactly(cabecera);

        Assert.True(FirmaImagen.Coincide(cabecera, FotosEjemplo.ContentType));
        Assert.InRange(FotosEjemplo.Tamano(muestra), 1, FotoEvaluacion.TamanoMaximoBytes);
    }

    [Fact]
    public void NombreArchivo_SeLeeDeVueltaConSuMuestra()
    {
        var nombre = FotosEjemplo.NombreArchivo("malla");

        Assert.StartsWith(FotosEjemplo.Prefijo, nombre);
        Assert.Equal("malla", FotosEjemplo.MuestraDe(nombre));
    }

    [Theory]
    [InlineData("solicitudes/5/abc.png")]
    [InlineData("datos-ejemplo/abc.jpg")]
    [InlineData("datos-ejemplo/seda/abc.png")]
    public void MuestraDe_NombreQueNoEsDeEjemplo_DevuelveNulo(string nombre)
    {
        Assert.Null(FotosEjemplo.MuestraDe(nombre));
    }

    [Fact]
    public void MuestraPara_TipoDeMaterial_UsaSuNombreEnMinusculas()
    {
        Assert.Equal("etiqueta", FotosEjemplo.MuestraPara(TipoMaterial.Crear("Etiqueta")));
    }
}
