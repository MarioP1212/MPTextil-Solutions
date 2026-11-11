using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace MPTextil.Web.Servicios;

public sealed class ClienteApi(HttpClient http, SesionActual sesion)
{
    private const string MensajeSinConexion = "No pudimos conectar con el servidor. Revisa tu conexión e intenta de nuevo en unos minutos.";
    private const string MensajeSinRespuesta = "El servidor tardó demasiado en responder. Intenta de nuevo en unos minutos.";
    private const string MensajeInesperado = "Ocurrió un problema inesperado. Intenta de nuevo y, si continúa, contacta al administrador.";
    private const string MensajeSesionExpirada = "Tu sesión expiró. Inicia sesión de nuevo.";

    public Task<ResultadoApi<T>> ObtenerAsync<T>(string ruta) =>
        EnviarAsync<T>(new HttpRequestMessage(HttpMethod.Get, ruta));

    public Task<ResultadoApi<T>> PublicarAsync<T>(string ruta, object? cuerpo = null) =>
        EnviarAsync<T>(new HttpRequestMessage(HttpMethod.Post, ruta)
        {
            Content = cuerpo is null ? null : JsonContent.Create(cuerpo, cuerpo.GetType(), options: OpcionesJson.Api)
        });

    public Task<ResultadoApi<T>> PublicarFormularioAsync<T>(string ruta, MultipartFormDataContent formulario) =>
        EnviarAsync<T>(new HttpRequestMessage(HttpMethod.Post, ruta) { Content = formulario });

    private async Task<ResultadoApi<T>> EnviarAsync<T>(HttpRequestMessage peticion)
    {
        using var mensaje = peticion;

        if (sesion.Expiro)
        {
            await sesion.CerrarAsync(MensajeSesionExpirada);
            return ResultadoApi<T>.Fallido(MensajeSesionExpirada);
        }

        var token = sesion.Token;

        if (token is not null)
        {
            mensaje.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        try
        {
            using var respuesta = await http.SendAsync(mensaje);

            if (respuesta.IsSuccessStatusCode)
            {
                return await LeerDatosAsync<T>(respuesta);
            }

            if (respuesta.StatusCode == HttpStatusCode.Unauthorized && token is not null)
            {
                await sesion.CerrarAsync(MensajeSesionExpirada);
            }

            return await LeerProblemaAsync<T>(respuesta);
        }
        catch (HttpRequestException)
        {
            return ResultadoApi<T>.Fallido(MensajeSinConexion);
        }
        catch (TaskCanceledException)
        {
            return ResultadoApi<T>.Fallido(MensajeSinRespuesta);
        }
    }

    private static async Task<ResultadoApi<T>> LeerDatosAsync<T>(HttpResponseMessage respuesta)
    {
        try
        {
            var datos = await respuesta.Content.ReadFromJsonAsync<T>(OpcionesJson.Api);
            return datos is null ? ResultadoApi<T>.Fallido(MensajeInesperado) : ResultadoApi<T>.Correcto(datos);
        }
        catch (Exception excepcion) when (excepcion is JsonException or NotSupportedException)
        {
            return ResultadoApi<T>.Fallido(MensajeInesperado);
        }
    }

    private static async Task<ResultadoApi<T>> LeerProblemaAsync<T>(HttpResponseMessage respuesta)
    {
        try
        {
            var problema = await respuesta.Content.ReadFromJsonAsync<DetalleProblema>(OpcionesJson.Api);
            var mensaje = problema?.Detail ?? problema?.Title ?? MensajeInesperado;

            return ResultadoApi<T>.Fallido(mensaje, problema?.Errors);
        }
        catch (Exception excepcion) when (excepcion is JsonException or NotSupportedException)
        {
            return ResultadoApi<T>.Fallido(MensajeInesperado);
        }
    }

    private sealed record DetalleProblema(string? Title, string? Detail, Dictionary<string, string[]>? Errors);
}
