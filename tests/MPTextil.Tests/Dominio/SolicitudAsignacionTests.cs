using MPTextil.Domain.Enumeraciones;
using MPTextil.Domain.Excepciones;
using MPTextil.Tests.Dominio.Soporte;

namespace MPTextil.Tests.Dominio;

public sealed class SolicitudAsignacionTests
{
    [Fact]
    public void Asignar_SolicitudSolicitada_QuedaAsignadaAlProveedor()
    {
        var solicitud = Escenarios.Solicitada();

        solicitud.Asignar(DatosPrueba.ProveedorAsignado(), Actores.Coordinador, DatosPrueba.EnDia());

        Assert.Equal(EstadoSolicitud.Asignado, solicitud.Estado);
        Assert.Equal(DatosPrueba.ProveedorAsignadoId, solicitud.ProveedorId);
        Assert.Contains("Textiles del Norte", solicitud.Historial.Last().Detalle);
    }

    [Fact]
    public void Asignar_ProveedorInactivo_LanzaReglaDeNegocio()
    {
        var solicitud = Escenarios.Solicitada();
        var proveedor = DatosPrueba.ProveedorAsignado();
        proveedor.Desactivar();

        var excepcion = Assert.Throws<ReglaNegocioException>(() => solicitud.Asignar(proveedor, Actores.Coordinador, DatosPrueba.EnDia()));

        Assert.Equal("El proveedor seleccionado no está activo.", excepcion.Message);
        Assert.Equal(EstadoSolicitud.Solicitado, solicitud.Estado);
    }

    [Fact]
    public void Asignar_SolicitudYaAsignada_LanzaTransicionInvalida()
    {
        var solicitud = Escenarios.Asignada();

        var excepcion = Assert.Throws<TransicionInvalidaException>(() =>
            solicitud.Asignar(DatosPrueba.OtroProveedor(), Actores.Coordinador, DatosPrueba.EnDia()));

        Assert.Equal("No es posible asignar la solicitud en estado Asignado.", excepcion.Message);
    }

    [Fact]
    public void Declinar_PorElProveedorAsignado_RegresaASolicitadoSinProveedor()
    {
        var solicitud = Escenarios.Asignada();

        solicitud.Declinar("Sin capacidad de producción este mes", Actores.Proveedor, DatosPrueba.EnDia());

        Assert.Equal(EstadoSolicitud.Solicitado, solicitud.Estado);
        Assert.Null(solicitud.ProveedorId);
        Assert.Contains("Sin capacidad de producción este mes", solicitud.Historial.Last().Detalle);
    }

    [Fact]
    public void Declinar_SinMotivo_LanzaReglaDeNegocio()
    {
        var solicitud = Escenarios.Asignada();

        Assert.Throws<ReglaNegocioException>(() => solicitud.Declinar(string.Empty, Actores.Proveedor, DatosPrueba.EnDia()));
        Assert.Equal(EstadoSolicitud.Asignado, solicitud.Estado);
    }

    [Fact]
    public void Declinar_PorOtroProveedor_LanzaAccesoDenegado()
    {
        var solicitud = Escenarios.Asignada();

        var excepcion = Assert.Throws<AccesoDenegadoException>(() =>
            solicitud.Declinar("No aplica", Actores.OtroProveedor, DatosPrueba.EnDia()));

        Assert.Equal("Esta solicitud no está asignada a tu empresa.", excepcion.Message);
    }

    [Fact]
    public void Asignar_DespuesDeDeclinar_PermiteElegirOtroProveedor()
    {
        var solicitud = Escenarios.Asignada();
        solicitud.Declinar("Sin capacidad", Actores.Proveedor, DatosPrueba.EnDia());

        solicitud.Asignar(DatosPrueba.OtroProveedor(), Actores.Coordinador, DatosPrueba.EnDia());

        Assert.Equal(EstadoSolicitud.Asignado, solicitud.Estado);
        Assert.Equal(DatosPrueba.OtroProveedorId, solicitud.ProveedorId);
    }

    [Fact]
    public void Aceptar_ConFechaValida_CreaUnCompromisoVigente()
    {
        var solicitud = Escenarios.Asignada();
        var fechaCompromiso = DatosPrueba.Hoy.AddDays(7);

        solicitud.Aceptar(fechaCompromiso, Actores.Proveedor, DatosPrueba.EnDia());

        Assert.Equal(EstadoSolicitud.EnProgreso, solicitud.Estado);
        Assert.NotNull(solicitud.CompromisoVigente);
        Assert.Equal(fechaCompromiso, solicitud.CompromisoVigente.FechaCompromiso);
        Assert.Equal(DatosPrueba.ProveedorAsignadoId, solicitud.CompromisoVigente.ProveedorId);
    }

    [Fact]
    public void Aceptar_ConFechaPasada_LanzaReglaDeNegocio()
    {
        var solicitud = Escenarios.Asignada();

        var excepcion = Assert.Throws<ReglaNegocioException>(() =>
            solicitud.Aceptar(DatosPrueba.Hoy.AddDays(-1), Actores.Proveedor, DatosPrueba.EnDia()));

        Assert.Equal("La fecha compromiso no puede ser anterior a hoy.", excepcion.Message);
        Assert.Empty(solicitud.Compromisos);
    }

    [Fact]
    public void Aceptar_PorOtroProveedor_LanzaAccesoDenegado()
    {
        var solicitud = Escenarios.Asignada();

        Assert.Throws<AccesoDenegadoException>(() =>
            solicitud.Aceptar(DatosPrueba.Hoy.AddDays(5), Actores.OtroProveedor, DatosPrueba.EnDia()));
    }

    [Fact]
    public void Aceptar_SolicitudSinAsignar_LanzaAccesoDenegado()
    {
        var solicitud = Escenarios.Solicitada();

        Assert.Throws<AccesoDenegadoException>(() =>
            solicitud.Aceptar(DatosPrueba.Hoy.AddDays(5), Actores.Proveedor, DatosPrueba.EnDia()));
    }
}
