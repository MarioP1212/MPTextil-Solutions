using MPTextil.Application.Abstracciones.Consultas;
using MPTextil.Domain.Entidades;
using MPTextil.Domain.Flujo;

namespace MPTextil.Application.Solicitudes.Consultas;

internal sealed class MapeoDetalleSolicitud(
    DatosDetalleSolicitud datos,
    Actor actor,
    DateOnly hoy,
    Func<string, string> urlDeFoto)
{
    private Solicitud Solicitud => datos.Solicitud;

    public SolicitudDetalle Crear() => new(
        Solicitud.Id,
        Solicitud.Codigo,
        Solicitud.NombreMaterial,
        Solicitud.Descripcion,
        Solicitud.TipoMaterial?.Nombre ?? string.Empty,
        Solicitud.Prioridad,
        Solicitud.Estado,
        Solicitud.Solicitante?.Nombre ?? MensajesSolicitud.UsuarioDesconocido,
        Solicitud.ProveedorId,
        Solicitud.Proveedor?.Nombre,
        Solicitud.FechaObjetivo,
        Solicitud.CompromisoVigente?.FechaCompromiso,
        Solicitud.EstaVencida(hoy),
        Solicitud.RequiereNuevaFechaCompromiso,
        Solicitud.FechaCreacion,
        Solicitud.FechaActualizacion,
        Solicitud.AccionesDisponibles(actor),
        Solicitud.Compromisos
            .OrderBy(compromiso => compromiso.FechaRegistro)
            .ThenBy(compromiso => compromiso.Id)
            .Select(CrearEntrega)
            .ToList(),
        Solicitud.Historial
            .OrderBy(registro => registro.Fecha)
            .ThenBy(registro => registro.Id)
            .Select(CrearHistorial)
            .ToList());

    private EntregaDetalle CrearEntrega(CompromisoEntrega compromiso) => new(
        compromiso.Id,
        compromiso.FechaCompromiso,
        EstadoDe(compromiso),
        Nombre(compromiso.RegistradoPorId),
        compromiso.FechaRegistro,
        compromiso.FechaReemplazo,
        compromiso.Envio is { } envio ? CrearEnvio(envio) : null);

    private EnvioDetalle CrearEnvio(Envio envio) => new(
        envio.FechaEnvio,
        envio.Transportista,
        envio.NumeroGuia,
        Nombre(envio.RegistradoPorId),
        envio.Evaluacion is { } evaluacion ? CrearEvaluacion(evaluacion) : null);

    private EvaluacionDetalle CrearEvaluacion(Evaluacion evaluacion) => new(
        evaluacion.Resultado,
        evaluacion.Comentarios,
        Nombre(evaluacion.EvaluadoPorId),
        evaluacion.FechaEvaluacion,
        evaluacion.Fotos
            .OrderBy(foto => foto.Id)
            .Select(foto => new FotoDetalle(foto.Id, foto.NombreOriginal, foto.ContentType, foto.TamanoBytes, urlDeFoto(foto.BlobNombre)))
            .ToList(),
        evaluacion.Decisiones
            .OrderBy(decision => decision.Nivel)
            .Select(decision => new DecisionDetalle(decision.Nivel, decision.Aprobada, decision.Motivo, Nombre(decision.UsuarioId), decision.Fecha))
            .ToList());

    private HistorialDetalle CrearHistorial(HistorialSolicitud registro) => new(
        registro.Fecha,
        registro.Usuario?.Nombre ?? MensajesSolicitud.UsuarioDesconocido,
        registro.Accion,
        registro.EstadoAnterior,
        registro.EstadoNuevo,
        registro.Detalle);

    private EstadoCompromiso EstadoDe(CompromisoEntrega compromiso) => compromiso switch
    {
        { Envio: not null } => compromiso.SeEntregoATiempo ? EstadoCompromiso.EntregadoATiempo : EstadoCompromiso.EntregadoTarde,
        { FechaReemplazo: not null } => compromiso.EstaVencido(hoy) ? EstadoCompromiso.ReemplazadoVencido : EstadoCompromiso.Reemplazado,
        _ => compromiso.EstaVencido(hoy) ? EstadoCompromiso.Vencido : EstadoCompromiso.Vigente
    };

    private string Nombre(int usuarioId) =>
        datos.NombresUsuarios.GetValueOrDefault(usuarioId, MensajesSolicitud.UsuarioDesconocido);
}
