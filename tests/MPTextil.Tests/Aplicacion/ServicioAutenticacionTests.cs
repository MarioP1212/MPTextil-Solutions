using MPTextil.Application.Autenticacion;
using MPTextil.Application.Excepciones;
using MPTextil.Domain.Entidades;
using MPTextil.Domain.Enumeraciones;
using MPTextil.Tests.Aplicacion.Soporte;
using MPTextil.Tests.Dominio.Soporte;

namespace MPTextil.Tests.Aplicacion;

public sealed class ServicioAutenticacionTests
{
    private const string Contrasena = "MPTextil2026!";
    private const int UsuarioId = 7;

    private static readonly DateTime FechaCreacion = new(2026, 11, 1, 12, 0, 0, DateTimeKind.Utc);

    private readonly HasherFalso _hasher = new();
    private readonly ServicioAutenticacion _servicio;

    public ServicioAutenticacionTests()
    {
        var usuario = Usuario.Crear(
            "Pedro López",
            "pedro.lopez@textilesdelnorte.example",
            HasherFalso.HashDe(Contrasena),
            Rol.Proveedor,
            DatosPrueba.ProveedorAsignadoId,
            FechaCreacion).ConId(UsuarioId);

        _servicio = new ServicioAutenticacion(
            new UsuarioRepositorioEnMemoria(usuario),
            _hasher,
            new GeneradorTokenFalso());
    }

    [Fact]
    public async Task IniciarSesion_ConCredencialesValidas_DevuelveTokenYPerfil()
    {
        var respuesta = await _servicio.IniciarSesionAsync(
            new SolicitudInicioSesion("pedro.lopez@textilesdelnorte.example", Contrasena),
            CancellationToken.None);

        Assert.Equal($"token-de-{UsuarioId}", respuesta.Token);
        Assert.Equal(GeneradorTokenFalso.Expiracion, respuesta.ExpiraUtc);
        Assert.Equal(UsuarioId, respuesta.Usuario.Id);
        Assert.Equal(Rol.Proveedor, respuesta.Usuario.Rol);
        Assert.Equal(DatosPrueba.ProveedorAsignadoId, respuesta.Usuario.ProveedorId);
    }

    [Fact]
    public async Task IniciarSesion_ConCorreoEnMayusculasYEspacios_EncuentraAlUsuario()
    {
        var respuesta = await _servicio.IniciarSesionAsync(
            new SolicitudInicioSesion("  Pedro.Lopez@TextilesDelNorte.example ", Contrasena),
            CancellationToken.None);

        Assert.Equal(UsuarioId, respuesta.Usuario.Id);
    }

    [Fact]
    public async Task IniciarSesion_ConContrasenaIncorrecta_LanzaCredencialesInvalidas()
    {
        var excepcion = await Assert.ThrowsAsync<CredencialesInvalidasException>(() => _servicio.IniciarSesionAsync(
            new SolicitudInicioSesion("pedro.lopez@textilesdelnorte.example", "otra-contrasena"),
            CancellationToken.None));

        Assert.Equal("El correo o la contraseña no son correctos.", excepcion.Message);
    }

    [Fact]
    public async Task IniciarSesion_ConCorreoInexistente_LanzaElMismoErrorYCalculaUnHash()
    {
        await Assert.ThrowsAsync<CredencialesInvalidasException>(() => _servicio.IniciarSesionAsync(
            new SolicitudInicioSesion("nadie@mptextil.example", Contrasena),
            CancellationToken.None));

        Assert.Equal(1, _hasher.HashesCalculados);
    }

    [Fact]
    public async Task ObtenerPerfil_UsuarioExistente_DevuelveSusDatos()
    {
        var perfil = await _servicio.ObtenerPerfilAsync(UsuarioId, CancellationToken.None);

        Assert.Equal("Pedro López", perfil.Nombre);
        Assert.Equal("pedro.lopez@textilesdelnorte.example", perfil.Correo);
    }

    [Fact]
    public async Task ObtenerPerfil_UsuarioInexistente_LanzaRecursoNoEncontrado()
    {
        await Assert.ThrowsAsync<RecursoNoEncontradoException>(() => _servicio.ObtenerPerfilAsync(999, CancellationToken.None));
    }
}
