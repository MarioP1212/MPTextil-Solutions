using System.Globalization;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Components.Forms;
using MPTextil.Web.Modelos;

namespace MPTextil.Web.Servicios;

public sealed class ApiSolicitudes(ClienteApi cliente)
{
    public const string FiltroVencidas = "vencidas";
    public const long TamanoMaximoFoto = 5 * 1024 * 1024;

    private const string Ruta = "api/solicitudes";

    public Task<ResultadoApi<List<SolicitudResumen>>> ListarAsync(string? filtro) =>
        cliente.ObtenerAsync<List<SolicitudResumen>>(string.IsNullOrEmpty(filtro) ? Ruta : $"{Ruta}/{filtro}");

    public Task<ResultadoApi<SolicitudDetalle>> ObtenerAsync(int id) =>
        cliente.ObtenerAsync<SolicitudDetalle>($"{Ruta}/{id}");

    public Task<ResultadoApi<RespuestaAccion>> CrearAsync(CrearSolicitud datos) =>
        cliente.PublicarAsync<RespuestaAccion>(Ruta, datos);

    public Task<ResultadoApi<RespuestaAccion>> AsignarAsync(int id, int proveedorId) =>
        Accion(id, "asignar", new AsignarProveedor(proveedorId));

    public Task<ResultadoApi<RespuestaAccion>> DeclinarAsync(int id, string motivo) =>
        Accion(id, "declinar", new MotivoAccion(motivo));

    public Task<ResultadoApi<RespuestaAccion>> AceptarAsync(int id, DateOnly fechaCompromiso) =>
        Accion(id, "aceptar", new FechaCompromisoSolicitud(fechaCompromiso));

    public Task<ResultadoApi<RespuestaAccion>> CambiarFechaCompromisoAsync(int id, DateOnly fechaCompromiso) =>
        Accion(id, "cambiar-fecha-compromiso", new FechaCompromisoSolicitud(fechaCompromiso));

    public Task<ResultadoApi<RespuestaAccion>> RegistrarEnvioAsync(int id, RegistrarEnvioSolicitud datos) =>
        Accion(id, "registrar-envio", datos);

    public Task<ResultadoApi<RespuestaAccion>> AprobarAsync(int id) => Accion(id, "aprobar");

    public Task<ResultadoApi<RespuestaAccion>> AutorizarAsync(int id) => Accion(id, "autorizar");

    public Task<ResultadoApi<RespuestaAccion>> RechazarAsync(int id, string motivo) =>
        Accion(id, "rechazar", new MotivoAccion(motivo));

    public async Task<ResultadoApi<RespuestaAccion>> RegistrarEvaluacionAsync(
        int id,
        ResultadoEvaluacion? resultado,
        string comentarios,
        IReadOnlyList<IBrowserFile> fotos)
    {
        using var formulario = new MultipartFormDataContent
        {
            { new StringContent(comentarios), "Comentarios" }
        };

        if (resultado is { } valor)
        {
            formulario.Add(new StringContent(valor.ToString()), "Resultado");
        }

        foreach (var foto in fotos)
        {
            var contenido = new StreamContent(foto.OpenReadStream(TamanoMaximoFoto));
            contenido.Headers.ContentType = new MediaTypeHeaderValue(foto.ContentType);
            formulario.Add(contenido, "Fotos", foto.Name);
        }

        return await cliente.PublicarFormularioAsync<RespuestaAccion>($"{Ruta}/{id.ToString(CultureInfo.InvariantCulture)}/registrar-evaluacion", formulario);
    }

    private Task<ResultadoApi<RespuestaAccion>> Accion(int id, string accion, object? cuerpo = null) =>
        cliente.PublicarAsync<RespuestaAccion>($"{Ruta}/{id.ToString(CultureInfo.InvariantCulture)}/{accion}", cuerpo);
}
