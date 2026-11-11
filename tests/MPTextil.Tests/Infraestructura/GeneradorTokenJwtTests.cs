using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using MPTextil.Application.Abstracciones.Seguridad;
using MPTextil.Domain.Entidades;
using MPTextil.Domain.Enumeraciones;
using MPTextil.Infrastructure.Seguridad;
using MPTextil.Tests.Dominio.Soporte;
using MPTextil.Tests.Infraestructura.Soporte;

namespace MPTextil.Tests.Infraestructura;

public sealed class GeneradorTokenJwtTests
{
    private static readonly DateTimeOffset Emitido = new(2026, 11, 10, 15, 0, 0, TimeSpan.Zero);

    private static readonly OpcionesJwt Opciones = new()
    {
        Emisor = "MPTextil.Api",
        Audiencia = "MPTextil.Web",
        Clave = "clave-de-pruebas-con-al-menos-32-caracteres",
        MinutosExpiracion = 60
    };

    private readonly GeneradorTokenJwt _generador = new(Options.Create(Opciones), new TiempoFijo(Emitido));

    [Fact]
    public void Generar_UsuarioProveedor_IncluyeSusReclamosYLaExpiracion()
    {
        var token = _generador.Generar(CrearUsuario(Rol.Proveedor, DatosPrueba.ProveedorAsignadoId));

        var jwt = new JsonWebToken(token.Valor);

        Assert.Equal("7", jwt.GetClaim(ReclamosUsuario.Id).Value);
        Assert.Equal("Pedro López", jwt.GetClaim(ReclamosUsuario.Nombre).Value);
        Assert.Equal("Proveedor", jwt.GetClaim(ReclamosUsuario.Rol).Value);
        Assert.Equal(DatosPrueba.ProveedorAsignadoId.ToString(), jwt.GetClaim(ReclamosUsuario.ProveedorId).Value);
        Assert.Equal(Opciones.Emisor, jwt.Issuer);
        Assert.Equal(Opciones.Audiencia, Assert.Single(jwt.Audiences));
        Assert.Equal(Emitido.UtcDateTime.AddMinutes(Opciones.MinutosExpiracion), jwt.ValidTo);
        Assert.Equal(jwt.ValidTo, token.ExpiraUtc);
    }

    [Fact]
    public void Generar_UsuarioInterno_NoIncluyeReclamoDeProveedor()
    {
        var token = _generador.Generar(CrearUsuario(Rol.Coordinador, null));

        var jwt = new JsonWebToken(token.Valor);

        Assert.False(jwt.TryGetClaim(ReclamosUsuario.ProveedorId, out _));
    }

    [Fact]
    public async Task Generar_TokenFirmado_SoloSeValidaConLaMismaClave()
    {
        var token = _generador.Generar(CrearUsuario(Rol.Solicitante, null));
        var manejador = new JsonWebTokenHandler();

        var conMismaClave = await manejador.ValidateTokenAsync(token.Valor, ParametrosValidacion(Opciones.CrearClaveFirma()));
        var conOtraClave = await manejador.ValidateTokenAsync(
            token.Valor,
            ParametrosValidacion(new OpcionesJwt { Clave = "otra-clave-distinta-con-mas-de-32-caracteres" }.CrearClaveFirma()));

        Assert.True(conMismaClave.IsValid);
        Assert.False(conOtraClave.IsValid);
    }

    private static Usuario CrearUsuario(Rol rol, int? proveedorId) =>
        Usuario.Crear("Pedro López", "pedro.lopez@textilesdelnorte.example", "hash", rol, proveedorId, Emitido.UtcDateTime).ConId(7);

    private static TokenValidationParameters ParametrosValidacion(SecurityKey clave) => new()
    {
        ValidIssuer = Opciones.Emisor,
        ValidAudience = Opciones.Audiencia,
        IssuerSigningKey = clave,
        ValidateLifetime = false
    };
}
