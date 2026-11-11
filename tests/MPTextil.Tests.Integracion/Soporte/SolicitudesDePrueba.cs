using System.Net;
using System.Net.Http.Headers;
using MPTextil.Application.Catalogos;
using MPTextil.Application.Solicitudes.Acciones;
using MPTextil.Application.Solicitudes.Consultas;
using MPTextil.Domain.Enumeraciones;

namespace MPTextil.Tests.Integracion.Soporte;

public sealed class SolicitudesDePrueba(ApiDePrueba api)
{
    public const int DiasCompromiso = 5;

    private static readonly byte[] PngDeUnPixel = Convert.FromBase64String(
        "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8BQDwAEhQGAhKmMIQAAAABJRU5ErkJggg==");

    public static ByteArrayContent Png() => Archivo(PngDeUnPixel, "image/png");

    public static ByteArrayContent Archivo(byte[] contenido, string contentType)
    {
        var archivo = new ByteArrayContent(contenido);
        archivo.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        return archivo;
    }

    public static MultipartFormDataContent Evaluacion(string resultado, string comentarios, params (HttpContent Contenido, string Nombre)[] fotos)
    {
        var formulario = new MultipartFormDataContent
        {
            { new StringContent(resultado), "Resultado" },
            { new StringContent(comentarios), "Comentarios" }
        };

        foreach (var (contenido, nombre) in fotos)
        {
            formulario.Add(contenido, "Fotos", nombre);
        }

        return formulario;
    }

    public async Task<int> CrearAsync()
    {
        using var sofia = await api.ClienteComoAsync(UsuariosDemo.Sofia);
        using var respuesta = await sofia.PublicarAsync("api/solicitudes", new CrearSolicitud(
            "Interlock 240 g/m² de prueba",
            "Tela interlock para polos de uniforme escolar.",
            1,
            Prioridad.Alta,
            api.Hoy.AddDays(20)));

        return await ComprobarAsync(respuesta, HttpStatusCode.Created, EstadoSolicitud.Solicitado);
    }

    public async Task<int> AsignadaAsync()
    {
        var id = await CrearAsync();
        using var carlos = await api.ClienteComoAsync(UsuariosDemo.Carlos);
        using var respuesta = await carlos.PublicarAsync($"api/solicitudes/{id}/asignar", new AsignarProveedor(await IdTextilesAsync()));

        return await ComprobarAsync(respuesta, HttpStatusCode.OK, EstadoSolicitud.Asignado);
    }

    public async Task<int> EnProgresoAsync()
    {
        var id = await AsignadaAsync();
        using var pedro = await api.ClienteComoAsync(UsuariosDemo.Pedro);
        using var respuesta = await pedro.PublicarAsync(
            $"api/solicitudes/{id}/aceptar",
            new FechaCompromisoSolicitud(api.Hoy.AddDays(DiasCompromiso)));

        return await ComprobarAsync(respuesta, HttpStatusCode.OK, EstadoSolicitud.EnProgreso);
    }

    public async Task<int> EnviadaAsync()
    {
        var id = await EnProgresoAsync();
        using var pedro = await api.ClienteComoAsync(UsuariosDemo.Pedro);
        using var respuesta = await pedro.PublicarAsync(
            $"api/solicitudes/{id}/registrar-envio",
            new RegistrarEnvioSolicitud(api.Hoy, "DHL", "DHL-PRUEBA-001"));

        return await ComprobarAsync(respuesta, HttpStatusCode.OK, EstadoSolicitud.Enviado);
    }

    public async Task<int> EnEvaluacionAsync()
    {
        var id = await EnviadaAsync();
        using var carlos = await api.ClienteComoAsync(UsuariosDemo.Carlos);
        using var formulario = Evaluacion("Cumple", "Muestra conforme a lo solicitado.", (Png(), "muestra.png"));
        using var respuesta = await carlos.PostAsync($"api/solicitudes/{id}/registrar-evaluacion", formulario);

        return await ComprobarAsync(respuesta, HttpStatusCode.OK, EstadoSolicitud.EnEvaluacion);
    }

    public async Task<int> PendienteDeAutorizacionAsync()
    {
        var id = await EnEvaluacionAsync();
        using var sofia = await api.ClienteComoAsync(UsuariosDemo.Sofia);
        using var respuesta = await sofia.PublicarAsync($"api/solicitudes/{id}/aprobar");

        return await ComprobarAsync(respuesta, HttpStatusCode.OK, EstadoSolicitud.PendienteAutorizacion);
    }

    public async Task<SolicitudDetalle> DetalleAsync(int id, string correo = UsuariosDemo.Carlos)
    {
        using var cliente = await api.ClienteComoAsync(correo);
        using var respuesta = await cliente.GetAsync($"api/solicitudes/{id}");
        respuesta.EnsureSuccessStatusCode();

        return await respuesta.LeerAsync<SolicitudDetalle>();
    }

    public async Task<int> IdTextilesAsync()
    {
        using var carlos = await api.ClienteComoAsync(UsuariosDemo.Carlos);
        using var respuesta = await carlos.GetAsync("api/catalogos/proveedores");
        var proveedores = await respuesta.LeerAsync<List<ElementoCatalogo>>();

        return proveedores.Single(proveedor => proveedor.Nombre == UsuariosDemo.TextilesDelNorte).Id;
    }

    private static async Task<int> ComprobarAsync(HttpResponseMessage respuesta, HttpStatusCode esperado, EstadoSolicitud estado)
    {
        Assert.Equal(esperado, respuesta.StatusCode);

        var accion = await respuesta.LeerAsync<RespuestaAccion>();
        Assert.Equal(estado, accion.Estado);

        return accion.SolicitudId;
    }
}
