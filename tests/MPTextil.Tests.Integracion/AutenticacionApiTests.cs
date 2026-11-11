using System.Net;
using System.Net.Http.Headers;
using MPTextil.Application.Autenticacion;
using MPTextil.Domain.Enumeraciones;
using MPTextil.Tests.Integracion.Soporte;

namespace MPTextil.Tests.Integracion;

[Collection(ColeccionApi.Nombre)]
public sealed class AutenticacionApiTests(ApiDePrueba api)
{
    private const string RutaInicioSesion = "api/autenticacion/iniciar-sesion";
    private const string RutaPerfil = "api/autenticacion/perfil";

    [Fact]
    public async Task IniciarSesion_ConCredencialesValidas_DevuelveTokenYPerfil()
    {
        using var cliente = api.ClienteAnonimo();

        using var respuesta = await cliente.PublicarAsync(RutaInicioSesion, new SolicitudInicioSesion("  PEDRO.Lopez@TextilesDelNorte.example ", UsuariosDemo.Contrasena));

        var sesion = await respuesta.LeerAsync<RespuestaInicioSesion>();
        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        Assert.False(string.IsNullOrWhiteSpace(sesion.Token));
        Assert.True(sesion.ExpiraUtc > DateTime.UtcNow);
        Assert.Equal(Rol.Proveedor, sesion.Usuario.Rol);
        Assert.Equal(UsuariosDemo.TextilesDelNorte, sesion.Usuario.Proveedor);
    }

    [Fact]
    public async Task IniciarSesion_ConContrasenaIncorrecta_Devuelve401EnEspanol()
    {
        using var cliente = api.ClienteAnonimo();

        using var respuesta = await cliente.PublicarAsync(RutaInicioSesion, new SolicitudInicioSesion(UsuariosDemo.Sofia, "incorrecta"));

        var problema = await respuesta.LeerProblemaAsync();
        Assert.Equal(HttpStatusCode.Unauthorized, respuesta.StatusCode);
        Assert.Equal("El correo o la contraseña no son correctos.", problema.Detail);
    }

    [Fact]
    public async Task IniciarSesion_SinDatos_Devuelve400ConErrorPorCampo()
    {
        using var cliente = api.ClienteAnonimo();

        using var respuesta = await cliente.PublicarAsync(RutaInicioSesion, new SolicitudInicioSesion(string.Empty, string.Empty));

        var problema = await respuesta.LeerProblemaAsync();
        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
        Assert.NotNull(problema.Errors);
        Assert.Equal(["Ingresa tu correo."], problema.Errors["Correo"]);
        Assert.Equal(["Ingresa tu contraseña."], problema.Errors["Contrasena"]);
    }

    [Fact]
    public async Task Perfil_SinToken_Devuelve401EnEspanol()
    {
        using var cliente = api.ClienteAnonimo();

        using var respuesta = await cliente.GetAsync(RutaPerfil);

        var problema = await respuesta.LeerProblemaAsync();
        Assert.Equal(HttpStatusCode.Unauthorized, respuesta.StatusCode);
        Assert.Equal("Tu sesión expiró o no es válida. Inicia sesión de nuevo.", problema.Detail);
    }

    [Fact]
    public async Task Perfil_ConTokenAlterado_Devuelve401()
    {
        using var cliente = await api.ClienteComoAsync(UsuariosDemo.Ana);
        var token = cliente.DefaultRequestHeaders.Authorization?.Parameter ?? string.Empty;
        cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token[..^4] + "AAAA");

        using var respuesta = await cliente.GetAsync(RutaPerfil);

        Assert.Equal(HttpStatusCode.Unauthorized, respuesta.StatusCode);
    }

    [Fact]
    public async Task Perfil_ConToken_DevuelveAlDuenoDelToken()
    {
        using var cliente = await api.ClienteComoAsync(UsuariosDemo.Ana);

        using var respuesta = await cliente.GetAsync(RutaPerfil);

        var perfil = await respuesta.LeerAsync<PerfilUsuario>();
        Assert.Equal("Ana Castillo", perfil.Nombre);
        Assert.Equal(Rol.Autorizador, perfil.Rol);
    }
}
