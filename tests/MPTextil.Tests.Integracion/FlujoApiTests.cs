using System.Net;
using MPTextil.Application.Solicitudes.Acciones;
using MPTextil.Application.Solicitudes.Consultas;
using MPTextil.Domain.Enumeraciones;
using MPTextil.Tests.Integracion.Soporte;

namespace MPTextil.Tests.Integracion;

[Collection(ColeccionApi.Nombre)]
public sealed class FlujoApiTests(ApiDePrueba api)
{
    private readonly SolicitudesDePrueba _solicitudes = new(api);

    [Fact]
    public async Task Crear_ComoSolicitante_Devuelve201ConUbicacionYCodigo()
    {
        using var sofia = await api.ClienteComoAsync(UsuariosDemo.Sofia);

        using var respuesta = await sofia.PublicarAsync("api/solicitudes", new CrearSolicitud(
            "Rib 2x2 para cuellos",
            "Tejido rib 2x2 con elastano para cuellos de polos.",
            1,
            Prioridad.Media,
            api.Hoy.AddDays(15)));

        var accion = await respuesta.LeerAsync<RespuestaAccion>();
        Assert.Equal(HttpStatusCode.Created, respuesta.StatusCode);
        Assert.Equal($"/api/solicitudes/{accion.SolicitudId}", respuesta.Headers.Location?.AbsolutePath);
        Assert.Matches("^SOL-\\d{5}$", accion.Codigo);
        Assert.Contains(accion.Codigo, accion.Mensaje);
    }

    [Fact]
    public async Task Crear_ComoCoordinador_Devuelve403()
    {
        using var carlos = await api.ClienteComoAsync(UsuariosDemo.Carlos);

        using var respuesta = await carlos.PublicarAsync("api/solicitudes", new CrearSolicitud(
            "Jersey", "Tela jersey", 1, Prioridad.Media, api.Hoy.AddDays(10)));

        var problema = await respuesta.LeerProblemaAsync();
        Assert.Equal(HttpStatusCode.Forbidden, respuesta.StatusCode);
        Assert.Equal("Tu usuario no tiene permiso para realizar esta acción.", problema.Detail);
    }

    [Fact]
    public async Task Crear_SinDatos_Devuelve400ConCadaCampo()
    {
        using var sofia = await api.ClienteComoAsync(UsuariosDemo.Sofia);

        using var respuesta = await sofia.PublicarAsync("api/solicitudes", new CrearSolicitud(string.Empty, string.Empty, 0, default, default));

        var problema = await respuesta.LeerProblemaAsync();
        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
        Assert.NotNull(problema.Errors);
        Assert.Equal(
            ["Descripcion", "FechaObjetivo", "NombreMaterial", "Prioridad", "TipoMaterialId"],
            problema.Errors.Keys.Order());
    }

    [Fact]
    public async Task FlujoCompleto_DeCrearAAutorizar_RecorreTodosLosEstados()
    {
        var id = await _solicitudes.PendienteDeAutorizacionAsync();
        using var ana = await api.ClienteComoAsync(UsuariosDemo.Ana);

        using var respuesta = await ana.PublicarAsync($"api/solicitudes/{id}/autorizar");

        var accion = await respuesta.LeerAsync<RespuestaAccion>();
        var detalle = await _solicitudes.DetalleAsync(id);
        var entrega = Assert.Single(detalle.Entregas);
        Assert.Equal(EstadoSolicitud.Aprobado, accion.Estado);
        Assert.Equal(EstadoCompromiso.EntregadoATiempo, entrega.Estado);
        Assert.Equal(2, entrega.Envio?.Evaluacion?.Decisiones.Count);
        Assert.Empty(detalle.AccionesDisponibles);
        Assert.Equal(
            [
                AccionSolicitud.Crear,
                AccionSolicitud.Asignar,
                AccionSolicitud.Aceptar,
                AccionSolicitud.RegistrarEnvio,
                AccionSolicitud.RegistrarEvaluacion,
                AccionSolicitud.Aprobar,
                AccionSolicitud.Autorizar
            ],
            detalle.Historial.Select(registro => registro.Accion));
    }

    [Fact]
    public async Task RegistrarEvaluacion_ConFotos_LasGuardaEnElAlmacenamiento()
    {
        var id = await _solicitudes.EnEvaluacionAsync();

        var detalle = await _solicitudes.DetalleAsync(id);

        var foto = Assert.Single(detalle.Entregas[^1].Envio?.Evaluacion?.Fotos ?? []);
        Assert.Equal("muestra.png", foto.NombreOriginal);
        Assert.Contains(api.Almacenamiento.Nombres, nombre => nombre.StartsWith($"solicitudes/{id}/", StringComparison.Ordinal));
    }

    [Fact]
    public async Task RegistrarEvaluacion_ConArchivoQueNoEsImagen_Devuelve400YNoSubeNada()
    {
        var id = await _solicitudes.EnviadaAsync();
        using var carlos = await api.ClienteComoAsync(UsuariosDemo.Carlos);
        var ejecutable = SolicitudesDePrueba.Archivo([0x4D, 0x5A, 0x90, 0x00, 0x03, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00], "image/jpeg");
        using var formulario = SolicitudesDePrueba.Evaluacion("Cumple", "Conforme", (ejecutable, "ficha.jpg"));

        using var respuesta = await carlos.PostAsync($"api/solicitudes/{id}/registrar-evaluacion", formulario);

        var problema = await respuesta.LeerProblemaAsync();
        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
        Assert.NotNull(problema.Errors);
        Assert.Equal(["El archivo ficha.jpg no es una imagen válida."], problema.Errors["Fotos[0]"]);
        Assert.DoesNotContain(api.Almacenamiento.Nombres, nombre => nombre.StartsWith($"solicitudes/{id}/", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Asignar_DosVeces_Devuelve409()
    {
        var id = await _solicitudes.AsignadaAsync();
        using var carlos = await api.ClienteComoAsync(UsuariosDemo.Carlos);

        using var respuesta = await carlos.PublicarAsync($"api/solicitudes/{id}/asignar", new AsignarProveedor(await _solicitudes.IdTextilesAsync()));

        var problema = await respuesta.LeerProblemaAsync();
        Assert.Equal(HttpStatusCode.Conflict, respuesta.StatusCode);
        Assert.Equal("No es posible asignar la solicitud en estado Asignado.", problema.Detail);
    }

    [Fact]
    public async Task Aceptar_PorOtroProveedor_Devuelve403()
    {
        var id = await _solicitudes.AsignadaAsync();
        using var lucia = await api.ClienteComoAsync(UsuariosDemo.Lucia);

        using var respuesta = await lucia.PublicarAsync($"api/solicitudes/{id}/aceptar", new FechaCompromisoSolicitud(api.Hoy.AddDays(3)));

        var problema = await respuesta.LeerProblemaAsync();
        Assert.Equal(HttpStatusCode.Forbidden, respuesta.StatusCode);
        Assert.Equal("Esta solicitud no está asignada a tu empresa.", problema.Detail);
    }

    [Fact]
    public async Task Rechazar_SinMotivo_Devuelve400()
    {
        var id = await _solicitudes.EnEvaluacionAsync();
        using var sofia = await api.ClienteComoAsync(UsuariosDemo.Sofia);

        using var respuesta = await sofia.PublicarAsync($"api/solicitudes/{id}/rechazar", new MotivoAccion(" "));

        var problema = await respuesta.LeerProblemaAsync();
        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
        Assert.NotNull(problema.Errors);
        Assert.Equal(["Escribe el motivo."], problema.Errors["Motivo"]);
    }

    [Fact]
    public async Task Rechazar_EnAutorizacion_RegresaAlProveedorConLaMismaFecha()
    {
        var id = await _solicitudes.PendienteDeAutorizacionAsync();
        using var ana = await api.ClienteComoAsync(UsuariosDemo.Ana);

        using var respuesta = await ana.PublicarAsync($"api/solicitudes/{id}/rechazar", new MotivoAccion("Falta la ficha de composición."));

        var accion = await respuesta.LeerAsync<RespuestaAccion>();
        var paraPedro = await _solicitudes.DetalleAsync(id, UsuariosDemo.Pedro);
        Assert.Equal(EstadoSolicitud.EnProgreso, accion.Estado);
        Assert.Equal(api.Hoy.AddDays(SolicitudesDePrueba.DiasCompromiso), paraPedro.FechaCompromiso);
        Assert.Equal([AccionSolicitud.CambiarFechaCompromiso, AccionSolicitud.RegistrarEnvio], paraPedro.AccionesDisponibles);
    }

    [Fact]
    public async Task Declinar_DejaLaSolicitudSinProveedorYElProveedorPierdeElAcceso()
    {
        var id = await _solicitudes.AsignadaAsync();
        using var pedro = await api.ClienteComoAsync(UsuariosDemo.Pedro);

        using var respuesta = await pedro.PublicarAsync($"api/solicitudes/{id}/declinar", new MotivoAccion("Sin capacidad este mes."));
        using var detalle = await pedro.GetAsync($"api/solicitudes/{id}");

        var accion = await respuesta.LeerAsync<RespuestaAccion>();
        Assert.Equal(EstadoSolicitud.Solicitado, accion.Estado);
        Assert.Equal(HttpStatusCode.NotFound, detalle.StatusCode);
    }
}
