using System.Text.Json;
using Microsoft.JSInterop;
using MPTextil.Web.Modelos;

namespace MPTextil.Web.Servicios;

public sealed class SesionActual(IJSRuntime js)
{
    private const string Clave = "mptextil.sesion";
    private const string MensajeSesionExpirada = "Tu sesión expiró. Inicia sesión de nuevo.";

    private DatosSesion? _datos;

    public event Action? Cambio;

    public PerfilUsuario? Usuario => _datos is { EstaVigente: true } datos ? datos.Usuario : null;

    public string? Token => _datos is { EstaVigente: true } datos ? datos.Token : null;

    public bool Expiro => _datos is { EstaVigente: false };

    public string? MotivoCierre { get; private set; }

    public async Task CargarAsync()
    {
        var guardado = await js.InvokeAsync<string?>("localStorage.getItem", Clave);
        _datos = Leer(guardado);

        if (_datos is null && !string.IsNullOrEmpty(guardado))
        {
            await js.InvokeVoidAsync("localStorage.removeItem", Clave);
        }
        else if (Expiro)
        {
            await CerrarAsync(MensajeSesionExpirada);
        }
    }

    public async Task IniciarAsync(RespuestaInicioSesion respuesta)
    {
        _datos = new DatosSesion(respuesta.Token, respuesta.ExpiraUtc, respuesta.Usuario);
        MotivoCierre = null;
        await js.InvokeVoidAsync("localStorage.setItem", Clave, JsonSerializer.Serialize(_datos, OpcionesJson.Api));
        Cambio?.Invoke();
    }

    public async Task CerrarAsync(string? motivo = null)
    {
        _datos = null;
        MotivoCierre = motivo;
        await js.InvokeVoidAsync("localStorage.removeItem", Clave);
        Cambio?.Invoke();
    }

    public string? TomarMotivoCierre()
    {
        var motivo = MotivoCierre;
        MotivoCierre = null;
        return motivo;
    }

    private static DatosSesion? Leer(string? guardado)
    {
        if (string.IsNullOrEmpty(guardado))
        {
            return null;
        }

        try
        {
            var datos = JsonSerializer.Deserialize<DatosSesion>(guardado, OpcionesJson.Api);
            return datos is { Token.Length: > 0, Usuario: not null } ? datos : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
