using MPTextil.Domain.Entidades;
using MPTextil.Domain.Enumeraciones;
using MPTextil.Domain.Excepciones;
using MPTextil.Tests.Dominio.Soporte;

namespace MPTextil.Tests.Dominio;

public sealed class EntidadesSimplesTests
{
    private static readonly DateTime FechaUtc = new(2026, 11, 10, 15, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void CrearUsuario_ProveedorConEmpresa_GeneraActorConSuEmpresa()
    {
        var usuario = Usuario.Crear("Ana López", "ana@textilesnorte.com", "hash", Rol.Proveedor, DatosPrueba.ProveedorAsignadoId, FechaUtc);

        var actor = usuario.ComoActor();

        Assert.Equal(Rol.Proveedor, actor.Rol);
        Assert.Equal(DatosPrueba.ProveedorAsignadoId, actor.ProveedorId);
        Assert.True(usuario.Activo);
    }

    [Fact]
    public void CrearUsuario_CorreoConMayusculas_LoGuardaEnMinusculas()
    {
        var usuario = Usuario.Crear("Ana López", "Ana.Lopez@MPTextil.com", "hash", Rol.Solicitante, null, FechaUtc);

        Assert.Equal("ana.lopez@mptextil.com", usuario.Correo);
    }

    [Fact]
    public void CrearUsuario_ProveedorSinEmpresa_LanzaReglaDeNegocio()
    {
        var excepcion = Assert.Throws<ReglaNegocioException>(() =>
            Usuario.Crear("Ana López", "ana@textilesnorte.com", "hash", Rol.Proveedor, null, FechaUtc));

        Assert.Equal("Un usuario con rol Proveedor debe pertenecer a un proveedor.", excepcion.Message);
    }

    [Theory]
    [InlineData(Rol.Solicitante)]
    [InlineData(Rol.Coordinador)]
    [InlineData(Rol.Autorizador)]
    public void CrearUsuario_RolInternoConEmpresa_LanzaReglaDeNegocio(Rol rol)
    {
        Assert.Throws<ReglaNegocioException>(() =>
            Usuario.Crear("Luis Pérez", "luis@mptextil.com", "hash", rol, DatosPrueba.ProveedorAsignadoId, FechaUtc));
    }

    [Theory]
    [InlineData("correo")]
    [InlineData("ana@")]
    [InlineData("ana lopez@mptextil.com")]
    public void CrearUsuario_ConCorreoInvalido_LanzaReglaDeNegocio(string correo)
    {
        var excepcion = Assert.Throws<ReglaNegocioException>(() =>
            Usuario.Crear("Ana López", correo, "hash", Rol.Solicitante, null, FechaUtc));

        Assert.Equal("El campo correo no tiene un formato de correo válido.", excepcion.Message);
    }

    [Fact]
    public void CrearProveedor_SinTelefono_LoDejaVacio()
    {
        var proveedor = Proveedor.Crear("Etiquetas Centroamericanas", "info@etiquetasca.com", "  ");

        Assert.Null(proveedor.Telefono);
        Assert.True(proveedor.Activo);
    }

    [Fact]
    public void DesactivarProveedor_LoMarcaComoInactivo()
    {
        var proveedor = DatosPrueba.ProveedorAsignado();

        proveedor.Desactivar();

        Assert.False(proveedor.Activo);
    }

    [Theory]
    [InlineData("image/jpeg")]
    [InlineData("image/png")]
    [InlineData("IMAGE/WEBP")]
    public void CrearFoto_ConTipoPermitido_GuardaElTipoEnMinusculas(string contentType)
    {
        var foto = FotoEvaluacion.Crear("muestras/a.jpg", "a.jpg", contentType, 2048, FechaUtc);

        Assert.Equal(contentType.ToLowerInvariant(), foto.ContentType);
    }

    [Theory]
    [InlineData("application/pdf")]
    [InlineData("image/gif")]
    [InlineData("")]
    public void CrearFoto_ConTipoNoPermitido_LanzaReglaDeNegocio(string contentType)
    {
        var excepcion = Assert.Throws<ReglaNegocioException>(() =>
            FotoEvaluacion.Crear("muestras/a.pdf", "a.pdf", contentType, 2048, FechaUtc));

        Assert.Equal("Solo se permiten fotos en formato JPG, PNG o WEBP.", excepcion.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(FotoEvaluacion.TamanoMaximoBytes + 1)]
    public void CrearFoto_ConTamanoFueraDeRango_LanzaReglaDeNegocio(long tamanoBytes)
    {
        Assert.Throws<ReglaNegocioException>(() =>
            FotoEvaluacion.Crear("muestras/a.jpg", "a.jpg", "image/jpeg", tamanoBytes, FechaUtc));
    }

    [Fact]
    public void CrearFoto_ConElTamanoMaximo_EsValida()
    {
        var foto = FotoEvaluacion.Crear("muestras/a.jpg", "a.jpg", "image/jpeg", FotoEvaluacion.TamanoMaximoBytes, FechaUtc);

        Assert.Equal(FotoEvaluacion.TamanoMaximoBytes, foto.TamanoBytes);
    }
}
