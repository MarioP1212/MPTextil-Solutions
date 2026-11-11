using MPTextil.Application.Abstracciones.Almacenamiento;
using MPTextil.Application.Abstracciones.Consultas;
using MPTextil.Application.Abstracciones.Seguridad;
using MPTextil.Application.Abstracciones.Tiempo;
using MPTextil.Application.Excepciones;
using MPTextil.Domain.Enumeraciones;

namespace MPTextil.Application.Solicitudes.Consultas;

public sealed class ServicioConsultaSolicitudes(
    IConsultasSolicitudes consultas,
    IUsuarioActual usuarioActual,
    IReloj reloj,
    IAlmacenamientoArchivos almacenamiento)
{
    public Task<IReadOnlyList<SolicitudResumen>> ListarAsync(EstadoSolicitud? estado, CancellationToken cancellationToken) =>
        consultas.ListarAsync(CrearFiltro() with { Estado = estado }, cancellationToken);

    public Task<IReadOnlyList<SolicitudResumen>> ListarVencidasAsync(CancellationToken cancellationToken) =>
        consultas.ListarAsync(CrearFiltro() with { SoloVencidas = true }, cancellationToken);

    public async Task<SolicitudDetalle> ObtenerDetalleAsync(int id, CancellationToken cancellationToken)
    {
        var actor = usuarioActual.Actor;
        var datos = await consultas.ObtenerDetalleAsync(id, cancellationToken);

        if (datos is null || !VisibilidadSolicitudes.PuedeVer(actor, datos.Solicitud))
        {
            throw new RecursoNoEncontradoException(MensajesSolicitud.NoEncontrada);
        }

        return new MapeoDetalleSolicitud(datos, actor, reloj.Ahora().Hoy, almacenamiento.ObtenerUrlLectura).Crear();
    }

    private FiltroSolicitudes CrearFiltro() =>
        new(reloj.Ahora().Hoy, VisibilidadSolicitudes.ProveedorVisible(usuarioActual.Actor));
}
