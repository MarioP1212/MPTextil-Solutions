using FluentValidation.Results;
using MPTextil.Application.Autenticacion;
using MPTextil.Application.Solicitudes.Acciones;
using MPTextil.Domain.Entidades;
using MPTextil.Domain.Enumeraciones;
using MPTextil.Tests.Aplicacion.Soporte;
using MPTextil.Tests.Dominio.Soporte;

namespace MPTextil.Tests.Aplicacion;

public sealed class ValidadoresTests
{
    private static readonly RelojFijo Reloj = new();

    [Fact]
    public void InicioSesion_SinDatos_IndicaCadaCampo()
    {
        var resultado = new ValidadorSolicitudInicioSesion().Validate(new SolicitudInicioSesion(string.Empty, string.Empty));

        Assert.Equal("Ingresa tu correo.", ErrorDe(resultado, nameof(SolicitudInicioSesion.Correo)));
        Assert.Equal("Ingresa tu contraseña.", ErrorDe(resultado, nameof(SolicitudInicioSesion.Contrasena)));
    }

    [Fact]
    public void InicioSesion_ConCorreoMalFormado_DaUnSoloMensaje()
    {
        var resultado = new ValidadorSolicitudInicioSesion().Validate(new SolicitudInicioSesion("pedro.lopez", "clave"));

        var error = Assert.Single(resultado.Errors);
        Assert.StartsWith("Ingresa un correo con un formato válido", error.ErrorMessage);
    }

    [Fact]
    public void CrearSolicitud_ConDatosValidos_NoTieneErrores()
    {
        var datos = new CrearSolicitud("Jersey 180 g/m²", "Tela jersey para camisetas", 1, Prioridad.Alta, DatosPrueba.Hoy);

        Assert.True(new ValidadorCrearSolicitud(Reloj).Validate(datos).IsValid);
    }

    [Fact]
    public void CrearSolicitud_SinDatos_IndicaCadaCampo()
    {
        var resultado = new ValidadorCrearSolicitud(Reloj).Validate(new CrearSolicitud(" ", string.Empty, 0, default, default));

        Assert.Equal("Escribe el nombre del material.", ErrorDe(resultado, nameof(CrearSolicitud.NombreMaterial)));
        Assert.Equal("Describe el material que necesitas.", ErrorDe(resultado, nameof(CrearSolicitud.Descripcion)));
        Assert.Equal("Selecciona el tipo de material.", ErrorDe(resultado, nameof(CrearSolicitud.TipoMaterialId)));
        Assert.Equal("Selecciona una prioridad válida.", ErrorDe(resultado, nameof(CrearSolicitud.Prioridad)));
        Assert.Equal("Indica la fecha objetivo.", ErrorDe(resultado, nameof(CrearSolicitud.FechaObjetivo)));
    }

    [Fact]
    public void CrearSolicitud_ConFechaObjetivoPasada_IndicaElError()
    {
        var datos = new CrearSolicitud("Jersey", "Tela", 1, Prioridad.Media, DatosPrueba.Hoy.AddDays(-1));

        var resultado = new ValidadorCrearSolicitud(Reloj).Validate(datos);

        Assert.Equal("La fecha objetivo no puede ser anterior a hoy.", ErrorDe(resultado, nameof(CrearSolicitud.FechaObjetivo)));
    }

    [Fact]
    public void CrearSolicitud_ConNombreMuyLargo_IndicaElLimite()
    {
        var datos = new CrearSolicitud(new string('a', Solicitud.NombreMaterialMaximo + 1), "Tela", 1, Prioridad.Media, DatosPrueba.Hoy);

        var resultado = new ValidadorCrearSolicitud(Reloj).Validate(datos);

        Assert.Equal(
            $"El nombre del material no puede superar {Solicitud.NombreMaterialMaximo} caracteres.",
            ErrorDe(resultado, nameof(CrearSolicitud.NombreMaterial)));
    }

    [Fact]
    public void AsignarProveedor_SinProveedor_IndicaElError()
    {
        var resultado = new ValidadorAsignarProveedor().Validate(new AsignarProveedor(0));

        Assert.Equal("Selecciona el proveedor.", ErrorDe(resultado, nameof(AsignarProveedor.ProveedorId)));
    }

    [Fact]
    public void MotivoAccion_Vacio_IndicaElError()
    {
        var resultado = new ValidadorMotivoAccion().Validate(new MotivoAccion("   "));

        Assert.Equal("Escribe el motivo.", ErrorDe(resultado, nameof(MotivoAccion.Motivo)));
    }

    [Theory]
    [InlineData(-1, "La fecha compromiso no puede ser anterior a hoy.")]
    [InlineData(null, "Indica la fecha compromiso.")]
    public void FechaCompromiso_FueraDeRango_IndicaElError(int? dias, string mensajeEsperado)
    {
        var fecha = dias is { } valor ? DatosPrueba.Hoy.AddDays(valor) : default;

        var resultado = new ValidadorFechaCompromisoSolicitud(Reloj).Validate(new FechaCompromisoSolicitud(fecha));

        Assert.Equal(mensajeEsperado, ErrorDe(resultado, nameof(FechaCompromisoSolicitud.FechaCompromiso)));
    }

    [Fact]
    public void RegistrarEnvio_ConFechaFuturaYSinGuia_IndicaCadaCampo()
    {
        var datos = new RegistrarEnvioSolicitud(DatosPrueba.Hoy.AddDays(1), "DHL", string.Empty);

        var resultado = new ValidadorRegistrarEnvioSolicitud(Reloj).Validate(datos);

        Assert.Equal("La fecha de envío no puede ser posterior a hoy.", ErrorDe(resultado, nameof(RegistrarEnvioSolicitud.FechaEnvio)));
        Assert.Equal("Escribe el número de guía.", ErrorDe(resultado, nameof(RegistrarEnvioSolicitud.NumeroGuia)));
        Assert.DoesNotContain(resultado.Errors, error => error.PropertyName == nameof(RegistrarEnvioSolicitud.Transportista));
    }

    private static string ErrorDe(ValidationResult resultado, string propiedad) =>
        Assert.Single(resultado.Errors, error => error.PropertyName == propiedad).ErrorMessage;
}
