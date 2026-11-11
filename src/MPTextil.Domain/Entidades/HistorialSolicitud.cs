using MPTextil.Domain.Comun;
using MPTextil.Domain.Enumeraciones;

namespace MPTextil.Domain.Entidades;

public sealed class HistorialSolicitud : Entidad<long>
{
    public const int DetalleMaximo = 1000;

    private HistorialSolicitud()
    {
    }

    public int SolicitudId { get; private set; }
    public int UsuarioId { get; private set; }
    public Usuario? Usuario { get; private set; }
    public EstadoSolicitud? EstadoAnterior { get; private set; }
    public EstadoSolicitud EstadoNuevo { get; private set; }
    public AccionSolicitud Accion { get; private set; }
    public string Detalle { get; private set; } = string.Empty;
    public DateTime Fecha { get; private set; }

    internal static HistorialSolicitud Registrar(
        AccionSolicitud accion,
        EstadoSolicitud? estadoAnterior,
        EstadoSolicitud estadoNuevo,
        int usuarioId,
        string detalle,
        DateTime fechaUtc) => new()
    {
        Accion = accion,
        EstadoAnterior = estadoAnterior,
        EstadoNuevo = estadoNuevo,
        UsuarioId = usuarioId,
        Detalle = Guardia.TextoObligatorio(detalle, "detalle del historial", DetalleMaximo),
        Fecha = fechaUtc
    };
}
