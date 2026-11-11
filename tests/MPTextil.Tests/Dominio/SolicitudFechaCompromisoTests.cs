using MPTextil.Domain.Enumeraciones;
using MPTextil.Domain.Excepciones;
using MPTextil.Tests.Dominio.Soporte;

namespace MPTextil.Tests.Dominio;

public sealed class SolicitudFechaCompromisoTests
{
    [Fact]
    public void Rechazar_ConFechaCompromisoPorVenir_ConservaLaMismaFecha()
    {
        var solicitud = Escenarios.RechazadaEnDia(0);

        Assert.Equal(2, solicitud.Compromisos.Count);
        Assert.NotNull(solicitud.CompromisoVigente);
        Assert.Equal(DatosPrueba.Hoy.AddDays(DatosPrueba.DiasCompromiso), solicitud.CompromisoVigente.FechaCompromiso);
        Assert.False(solicitud.RequiereNuevaFechaCompromiso);
    }

    [Fact]
    public void Rechazar_ConFechaCompromisoVencida_RequiereUnaNuevaFecha()
    {
        var solicitud = Escenarios.RechazadaEnDia(DatosPrueba.DiasCompromiso + 1);

        Assert.Single(solicitud.Compromisos);
        Assert.Null(solicitud.CompromisoVigente);
        Assert.True(solicitud.RequiereNuevaFechaCompromiso);
        Assert.False(solicitud.EstaVencida(DatosPrueba.Hoy.AddDays(DatosPrueba.DiasCompromiso + 1)));
    }

    [Fact]
    public void RegistrarEnvio_SinCompromisoVigente_LanzaReglaDeNegocio()
    {
        var dia = DatosPrueba.DiasCompromiso + 1;
        var solicitud = Escenarios.RechazadaEnDia(dia);

        var excepcion = Assert.Throws<ReglaNegocioException>(() =>
            solicitud.RegistrarEnvio(DatosPrueba.Hoy.AddDays(dia), "DHL", "GUIA-002", Actores.Proveedor, DatosPrueba.EnDia(dia)));

        Assert.Equal("Registra una nueva fecha compromiso antes de enviar la muestra.", excepcion.Message);
    }

    [Fact]
    public void CambiarFechaCompromiso_SinRechazoPrevio_LanzaReglaDeNegocio()
    {
        var solicitud = Escenarios.EnProgreso();

        var excepcion = Assert.Throws<ReglaNegocioException>(() =>
            solicitud.CambiarFechaCompromiso(DatosPrueba.Hoy.AddDays(10), Actores.Proveedor, DatosPrueba.EnDia()));

        Assert.Equal("Solo puedes cambiar la fecha compromiso después de que la muestra fue rechazada.", excepcion.Message);
    }

    [Fact]
    public void CambiarFechaCompromiso_TrasUnRechazo_ReemplazaElCompromisoAnterior()
    {
        var solicitud = Escenarios.RechazadaEnDia(0);
        var anterior = solicitud.CompromisoVigente;
        var nuevaFecha = DatosPrueba.Hoy.AddDays(12);

        solicitud.CambiarFechaCompromiso(nuevaFecha, Actores.Proveedor, DatosPrueba.EnDia(2));

        Assert.NotNull(anterior);
        Assert.Equal(DatosPrueba.Hoy.AddDays(2), anterior.FechaReemplazo);
        Assert.NotNull(solicitud.CompromisoVigente);
        Assert.Equal(nuevaFecha, solicitud.CompromisoVigente.FechaCompromiso);
        Assert.Equal(EstadoSolicitud.EnProgreso, solicitud.Estado);
    }

    [Fact]
    public void CambiarFechaCompromiso_SinCompromisoVigente_RegistraLaNuevaFecha()
    {
        var dia = DatosPrueba.DiasCompromiso + 1;
        var solicitud = Escenarios.RechazadaEnDia(dia);
        var nuevaFecha = DatosPrueba.Hoy.AddDays(dia + 7);

        solicitud.CambiarFechaCompromiso(nuevaFecha, Actores.Proveedor, DatosPrueba.EnDia(dia));

        Assert.NotNull(solicitud.CompromisoVigente);
        Assert.Equal(nuevaFecha, solicitud.CompromisoVigente.FechaCompromiso);
        Assert.False(solicitud.RequiereNuevaFechaCompromiso);
    }

    [Fact]
    public void CambiarFechaCompromiso_ConLaMismaFecha_LanzaReglaDeNegocio()
    {
        var solicitud = Escenarios.RechazadaEnDia(0);
        var mismaFecha = DatosPrueba.Hoy.AddDays(DatosPrueba.DiasCompromiso);

        var excepcion = Assert.Throws<ReglaNegocioException>(() =>
            solicitud.CambiarFechaCompromiso(mismaFecha, Actores.Proveedor, DatosPrueba.EnDia()));

        Assert.Equal("La nueva fecha compromiso es igual a la vigente.", excepcion.Message);
    }

    [Fact]
    public void CambiarFechaCompromiso_ConFechaPasada_LanzaReglaDeNegocio()
    {
        var solicitud = Escenarios.RechazadaEnDia(0);

        Assert.Throws<ReglaNegocioException>(() =>
            solicitud.CambiarFechaCompromiso(DatosPrueba.Hoy.AddDays(1), Actores.Proveedor, DatosPrueba.EnDia(2)));
    }

    [Fact]
    public void CambiarFechaCompromiso_AntesDeVencer_NoCuentaComoVencido()
    {
        var solicitud = Escenarios.RechazadaEnDia(0);
        var anterior = solicitud.CompromisoVigente;

        solicitud.CambiarFechaCompromiso(DatosPrueba.Hoy.AddDays(10), Actores.Proveedor, DatosPrueba.EnDia(3));

        Assert.NotNull(anterior);
        Assert.False(anterior.EstaVencido(DatosPrueba.Hoy.AddDays(20)));
    }

    [Fact]
    public void CambiarFechaCompromiso_DespuesDeVencer_CuentaComoVencido()
    {
        var solicitud = Escenarios.RechazadaEnDia(0);
        var anterior = solicitud.CompromisoVigente;

        solicitud.CambiarFechaCompromiso(DatosPrueba.Hoy.AddDays(15), Actores.Proveedor, DatosPrueba.EnDia(8));

        Assert.NotNull(anterior);
        Assert.True(anterior.EstaVencido(DatosPrueba.Hoy.AddDays(20)));
    }

    [Theory]
    [InlineData(4, false)]
    [InlineData(5, false)]
    [InlineData(6, true)]
    public void EstaVencida_SegunElDiaDeConsulta_DevuelveElResultadoEsperado(int dia, bool esperado)
    {
        var solicitud = Escenarios.EnProgreso(diasCompromiso: 5);

        var vencida = solicitud.EstaVencida(DatosPrueba.Hoy.AddDays(dia));

        Assert.Equal(esperado, vencida);
    }

    [Fact]
    public void EstaVencida_SolicitudYaEnviada_DevuelveFalso()
    {
        var solicitud = Escenarios.Enviada();

        Assert.False(solicitud.EstaVencida(DatosPrueba.Hoy.AddDays(30)));
    }
}
