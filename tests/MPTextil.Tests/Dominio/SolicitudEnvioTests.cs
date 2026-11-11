using MPTextil.Domain.Enumeraciones;
using MPTextil.Domain.Excepciones;
using MPTextil.Tests.Dominio.Soporte;

namespace MPTextil.Tests.Dominio;

public sealed class SolicitudEnvioTests
{
    [Fact]
    public void RegistrarEnvio_ConDatosValidos_QuedaEnviadaYCierraElCompromiso()
    {
        var solicitud = Escenarios.EnProgreso();

        solicitud.RegistrarEnvio(DatosPrueba.Hoy, "DHL", "GUIA-001", Actores.Proveedor, DatosPrueba.EnDia());

        var compromiso = Assert.Single(solicitud.Compromisos);
        Assert.Equal(EstadoSolicitud.Enviado, solicitud.Estado);
        Assert.Null(solicitud.CompromisoVigente);
        Assert.NotNull(compromiso.Envio);
        Assert.Equal("DHL", compromiso.Envio.Transportista);
        Assert.Equal("GUIA-001", compromiso.Envio.NumeroGuia);
    }

    [Fact]
    public void RegistrarEnvio_AntesDeLaFechaCompromiso_SeEntregaATiempo()
    {
        var solicitud = Escenarios.EnProgreso(diasCompromiso: 5);

        solicitud.RegistrarEnvio(DatosPrueba.Hoy.AddDays(5), "DHL", "GUIA-001", Actores.Proveedor, DatosPrueba.EnDia(5));

        Assert.True(Assert.Single(solicitud.Compromisos).SeEntregoATiempo);
    }

    [Fact]
    public void RegistrarEnvio_DespuesDeLaFechaCompromiso_NoSeEntregaATiempo()
    {
        var solicitud = Escenarios.EnProgreso(diasCompromiso: 2);

        solicitud.RegistrarEnvio(DatosPrueba.Hoy.AddDays(4), "DHL", "GUIA-001", Actores.Proveedor, DatosPrueba.EnDia(4));

        Assert.False(Assert.Single(solicitud.Compromisos).SeEntregoATiempo);
    }

    [Fact]
    public void RegistrarEnvio_ConFechaFutura_LanzaReglaDeNegocio()
    {
        var solicitud = Escenarios.EnProgreso();

        var excepcion = Assert.Throws<ReglaNegocioException>(() =>
            solicitud.RegistrarEnvio(DatosPrueba.Hoy.AddDays(1), "DHL", "GUIA-001", Actores.Proveedor, DatosPrueba.EnDia()));

        Assert.Equal("La fecha de envío no puede ser posterior a hoy.", excepcion.Message);
        Assert.Equal(EstadoSolicitud.EnProgreso, solicitud.Estado);
    }

    [Theory]
    [InlineData("", "GUIA-001", "El campo transportista es obligatorio.")]
    [InlineData("DHL", " ", "El campo número de guía es obligatorio.")]
    public void RegistrarEnvio_SinDatosDelEnvio_LanzaReglaDeNegocio(string transportista, string numeroGuia, string mensajeEsperado)
    {
        var solicitud = Escenarios.EnProgreso();

        var excepcion = Assert.Throws<ReglaNegocioException>(() =>
            solicitud.RegistrarEnvio(DatosPrueba.Hoy, transportista, numeroGuia, Actores.Proveedor, DatosPrueba.EnDia()));

        Assert.Equal(mensajeEsperado, excepcion.Message);
        Assert.NotNull(solicitud.CompromisoVigente);
    }

    [Fact]
    public void RegistrarEnvio_SolicitudSinAceptar_LanzaTransicionInvalida()
    {
        var solicitud = Escenarios.Asignada();

        var excepcion = Assert.Throws<TransicionInvalidaException>(() =>
            solicitud.RegistrarEnvio(DatosPrueba.Hoy, "DHL", "GUIA-001", Actores.Proveedor, DatosPrueba.EnDia()));

        Assert.Equal("No es posible registrar el envío en estado Asignado.", excepcion.Message);
    }

    [Fact]
    public void RegistrarEnvio_PorCoordinador_LanzaAccesoDenegado()
    {
        var solicitud = Escenarios.EnProgreso();

        Assert.Throws<AccesoDenegadoException>(() =>
            solicitud.RegistrarEnvio(DatosPrueba.Hoy, "DHL", "GUIA-001", Actores.Coordinador, DatosPrueba.EnDia()));
    }
}
