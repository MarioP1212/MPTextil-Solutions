using MPTextil.Application.Abstracciones.Almacenamiento;
using MPTextil.Application.Abstracciones.Persistencia;
using MPTextil.Application.Abstracciones.Seguridad;
using MPTextil.Application.Abstracciones.Tiempo;
using MPTextil.Application.Excepciones;
using MPTextil.Domain.Comun;
using MPTextil.Domain.Entidades;
using MPTextil.Domain.Enumeraciones;
using MPTextil.Domain.Flujo;

namespace MPTextil.Application.Solicitudes.Acciones;

public sealed class ServicioSolicitudes(
    ISolicitudRepositorio solicitudes,
    IProveedorRepositorio proveedores,
    ITipoMaterialRepositorio tiposMaterial,
    IUnidadDeTrabajo unidadDeTrabajo,
    IUsuarioActual usuarioActual,
    IReloj reloj,
    IAlmacenamientoArchivos almacenamiento)
{
    public async Task<RespuestaAccion> CrearAsync(CrearSolicitud datos, CancellationToken cancellationToken)
    {
        var tipoMaterial = await tiposMaterial.ObtenerPorIdAsync(datos.TipoMaterialId, cancellationToken)
            ?? throw ValidacionException.DeCampo(nameof(CrearSolicitud.TipoMaterialId), "El tipo de material seleccionado no existe.");

        var solicitud = Solicitud.Crear(
            datos.NombreMaterial,
            datos.Descripcion,
            tipoMaterial,
            datos.Prioridad,
            datos.FechaObjetivo,
            usuarioActual.Actor,
            reloj.Ahora());

        solicitudes.Agregar(solicitud);

        return await GuardarAsync(
            solicitud,
            creada => $"Se creó la solicitud {creada.Codigo}. Un coordinador la asignará a un proveedor.",
            cancellationToken);
    }

    public async Task<RespuestaAccion> AsignarAsync(int id, AsignarProveedor datos, CancellationToken cancellationToken)
    {
        var solicitud = await ObtenerAsync(id, cancellationToken);
        var proveedor = await proveedores.ObtenerPorIdAsync(datos.ProveedorId, cancellationToken)
            ?? throw ValidacionException.DeCampo(nameof(AsignarProveedor.ProveedorId), "El proveedor seleccionado no existe.");

        solicitud.Asignar(proveedor, usuarioActual.Actor, reloj.Ahora());

        return await GuardarAsync(
            solicitud,
            asignada => $"La solicitud {asignada.Codigo} se asignó a {proveedor.Nombre}.",
            cancellationToken);
    }

    public Task<RespuestaAccion> DeclinarAsync(int id, MotivoAccion datos, CancellationToken cancellationToken) =>
        EjecutarAsync(
            id,
            (solicitud, actor, momento) => solicitud.Declinar(datos.Motivo, actor, momento),
            solicitud => $"Declinaste la solicitud {solicitud.Codigo}. El coordinador la asignará a otro proveedor.",
            cancellationToken);

    public Task<RespuestaAccion> AceptarAsync(int id, FechaCompromisoSolicitud datos, CancellationToken cancellationToken) =>
        EjecutarAsync(
            id,
            (solicitud, actor, momento) => solicitud.Aceptar(datos.FechaCompromiso, actor, momento),
            solicitud => $"Aceptaste la solicitud {solicitud.Codigo} con fecha compromiso {datos.FechaCompromiso.ATexto()}.",
            cancellationToken);

    public Task<RespuestaAccion> CambiarFechaCompromisoAsync(int id, FechaCompromisoSolicitud datos, CancellationToken cancellationToken) =>
        EjecutarAsync(
            id,
            (solicitud, actor, momento) => solicitud.CambiarFechaCompromiso(datos.FechaCompromiso, actor, momento),
            solicitud => $"La nueva fecha compromiso de {solicitud.Codigo} es el {datos.FechaCompromiso.ATexto()}.",
            cancellationToken);

    public Task<RespuestaAccion> RegistrarEnvioAsync(int id, RegistrarEnvioSolicitud datos, CancellationToken cancellationToken) =>
        EjecutarAsync(
            id,
            (solicitud, actor, momento) => solicitud.RegistrarEnvio(datos.FechaEnvio, datos.Transportista, datos.NumeroGuia, actor, momento),
            solicitud => $"Se registró el envío de la muestra de {solicitud.Codigo}. El coordinador la evaluará cuando la reciba.",
            cancellationToken);

    public async Task<RespuestaAccion> RegistrarEvaluacionAsync(int id, RegistrarEvaluacion datos, CancellationToken cancellationToken)
    {
        var solicitud = await ObtenerAsync(id, cancellationToken);
        var actor = usuarioActual.Actor;
        var momento = reloj.Ahora();
        solicitud.ValidarAccion(AccionSolicitud.RegistrarEvaluacion, actor);

        var archivosSubidos = new List<string>();

        try
        {
            var fotos = new List<FotoEvaluacion>();

            foreach (var archivo in datos.Fotos)
            {
                var nombre = $"solicitudes/{solicitud.Id}/{Guid.NewGuid():N}{FotoEvaluacion.ExtensionDe(archivo.ContentType)}";
                var foto = FotoEvaluacion.Crear(nombre, archivo.NombreOriginal, archivo.ContentType, archivo.TamanoBytes, momento.Utc);

                await using (var contenido = archivo.AbrirLectura())
                {
                    await almacenamiento.GuardarAsync(nombre, contenido, foto.ContentType, cancellationToken);
                }

                archivosSubidos.Add(nombre);
                fotos.Add(foto);
            }

            solicitud.RegistrarEvaluacion(datos.Resultado, datos.Comentarios, fotos, actor, momento);
            await unidadDeTrabajo.GuardarCambiosAsync(cancellationToken);
        }
        catch
        {
            await EliminarArchivosAsync(archivosSubidos);
            throw;
        }

        var cantidad = datos.Fotos.Count == 1 ? "1 foto" : $"{datos.Fotos.Count} fotos";

        return RespuestaAccion.Desde(
            solicitud,
            $"Se registró la evaluación de {solicitud.Codigo} con {cantidad}. Ahora el solicitante debe aprobar o rechazar la muestra.");
    }

    public Task<RespuestaAccion> AprobarAsync(int id, CancellationToken cancellationToken) =>
        EjecutarAsync(
            id,
            (solicitud, actor, momento) => solicitud.Aprobar(actor, momento),
            solicitud => $"Aprobaste la muestra de {solicitud.Codigo}. Queda pendiente de autorización.",
            cancellationToken);

    public Task<RespuestaAccion> AutorizarAsync(int id, CancellationToken cancellationToken) =>
        EjecutarAsync(
            id,
            (solicitud, actor, momento) => solicitud.Autorizar(actor, momento),
            solicitud => $"Autorizaste la muestra de {solicitud.Codigo}. La solicitud queda cerrada.",
            cancellationToken);

    public Task<RespuestaAccion> RechazarAsync(int id, MotivoAccion datos, CancellationToken cancellationToken) =>
        EjecutarAsync(
            id,
            (solicitud, actor, momento) => solicitud.Rechazar(datos.Motivo, actor, momento),
            MensajeRechazo,
            cancellationToken);

    private static string MensajeRechazo(Solicitud solicitud) => solicitud.CompromisoVigente is { } vigente
        ? $"Rechazaste la muestra de {solicitud.Codigo}. Regresó al proveedor con fecha compromiso {vigente.FechaCompromiso.ATexto()}."
        : $"Rechazaste la muestra de {solicitud.Codigo}. Regresó al proveedor, que debe registrar una nueva fecha compromiso.";

    private async Task<RespuestaAccion> EjecutarAsync(
        int id,
        Action<Solicitud, Actor, Momento> accion,
        Func<Solicitud, string> mensaje,
        CancellationToken cancellationToken)
    {
        var solicitud = await ObtenerAsync(id, cancellationToken);
        accion(solicitud, usuarioActual.Actor, reloj.Ahora());

        return await GuardarAsync(solicitud, mensaje, cancellationToken);
    }

    private async Task EliminarArchivosAsync(IEnumerable<string> nombres)
    {
        foreach (var nombre in nombres)
        {
            await almacenamiento.EliminarSiExisteAsync(nombre, CancellationToken.None);
        }
    }

    private async Task<Solicitud> ObtenerAsync(int id, CancellationToken cancellationToken) =>
        await solicitudes.ObtenerPorIdAsync(id, cancellationToken)
        ?? throw new RecursoNoEncontradoException(MensajesSolicitud.NoEncontrada);

    private async Task<RespuestaAccion> GuardarAsync(
        Solicitud solicitud,
        Func<Solicitud, string> mensaje,
        CancellationToken cancellationToken)
    {
        await unidadDeTrabajo.GuardarCambiosAsync(cancellationToken);

        return RespuestaAccion.Desde(solicitud, mensaje(solicitud));
    }
}
