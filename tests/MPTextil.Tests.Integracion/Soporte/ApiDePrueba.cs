using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using MPTextil.Application.Abstracciones.Tiempo;
using MPTextil.Application.Autenticacion;
using MPTextil.Infrastructure.Persistencia;

namespace MPTextil.Tests.Integracion.Soporte;

public sealed class ApiDePrueba : IAsyncLifetime
{
    private const string VariableServidor = "MPTEXTIL_PRUEBAS_SERVIDOR_SQL";
    private const string ServidorPorDefecto = @"(localdb)\MSSQLLocalDB";

    private readonly Dictionary<string, string> _tokens = [];
    private readonly FabricaApi _fabrica;

    public ApiDePrueba()
    {
        var servidor = Environment.GetEnvironmentVariable(VariableServidor) ?? ServidorPorDefecto;
        BaseDatos = $"MPTextil_Integracion_{Guid.NewGuid():N}";
        _fabrica = new FabricaApi(
            $"Server={servidor};Database={BaseDatos};Trusted_Connection=True;TrustServerCertificate=True",
            Almacenamiento);
    }

    public static JsonSerializerOptions Json { get; } = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public string BaseDatos { get; }

    public AlmacenamientoEnMemoria Almacenamiento { get; } = new();

    public DateOnly Hoy => _fabrica.Services.GetRequiredService<IReloj>().Ahora().Hoy;

    public Task InitializeAsync()
    {
        using var cliente = _fabrica.CreateClient();
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await using (var alcance = _fabrica.Services.CreateAsyncScope())
        {
            await alcance.ServiceProvider.GetRequiredService<MPTextilDbContext>().Database.EnsureDeletedAsync();
        }

        await _fabrica.DisposeAsync();
    }

    public HttpClient ClienteAnonimo() => _fabrica.CreateClient();

    public async Task<HttpClient> ClienteComoAsync(string correo)
    {
        if (!_tokens.TryGetValue(correo, out var token))
        {
            using var anonimo = ClienteAnonimo();
            using var respuesta = await anonimo.PostAsJsonAsync(
                "api/autenticacion/iniciar-sesion",
                new SolicitudInicioSesion(correo, UsuariosDemo.Contrasena),
                Json);
            respuesta.EnsureSuccessStatusCode();

            var sesion = await respuesta.Content.ReadFromJsonAsync<RespuestaInicioSesion>(Json)
                ?? throw new InvalidOperationException($"El inicio de sesión de {correo} no devolvió datos.");
            token = sesion.Token;
            _tokens[correo] = token;
        }

        var cliente = ClienteAnonimo();
        cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return cliente;
    }
}
