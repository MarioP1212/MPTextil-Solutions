using System.Net.Http.Json;

namespace MPTextil.Tests.Integracion.Soporte;

public sealed record Problema(string? Title, string? Detail, int? Status, Dictionary<string, string[]>? Errors);

public static class ExtensionesHttp
{
    public static async Task<T> LeerAsync<T>(this HttpResponseMessage respuesta) =>
        await respuesta.Content.ReadFromJsonAsync<T>(ApiDePrueba.Json)
        ?? throw new InvalidOperationException($"La respuesta de {respuesta.RequestMessage?.RequestUri} venía vacía.");

    public static Task<Problema> LeerProblemaAsync(this HttpResponseMessage respuesta) => respuesta.LeerAsync<Problema>();

    public static Task<HttpResponseMessage> PublicarAsync(this HttpClient cliente, string ruta, object? cuerpo = null) =>
        cliente.PostAsJsonAsync(ruta, cuerpo, ApiDePrueba.Json);
}
