using MPTextil.Domain.Comun;
using MPTextil.Domain.Entidades;
using MPTextil.Domain.Enumeraciones;
using MPTextil.Domain.Flujo;

namespace MPTextil.Infrastructure.Persistencia.DatosEjemplo;

internal sealed record ParticipantesEjemplo(
    Momento Ahora,
    Actor Coordinador,
    Actor Autorizador,
    IReadOnlyDictionary<int, Actor> UsuariosPorProveedor);

internal sealed class CronologiaSolicitud
{
    private readonly ParticipantesEjemplo _participantes;
    private readonly Actor _solicitante;
    private readonly string _muestraFoto;

    private CronologiaSolicitud(Solicitud solicitud, Actor solicitante, ParticipantesEjemplo participantes, string muestraFoto)
    {
        Solicitud = solicitud;
        _solicitante = solicitante;
        _participantes = participantes;
        _muestraFoto = muestraFoto;
    }

    public Solicitud Solicitud { get; }

    private Actor UsuarioProveedor => Solicitud.ProveedorId is { } proveedorId
        ? _participantes.UsuariosPorProveedor[proveedorId]
        : throw new InvalidOperationException("La solicitud de ejemplo no tiene proveedor asignado.");

    public static CronologiaSolicitud Crear(
        ParticipantesEjemplo participantes,
        Actor solicitante,
        string nombreMaterial,
        string descripcion,
        TipoMaterial tipoMaterial,
        Prioridad prioridad,
        int plazoDias,
        int dia)
    {
        var momento = EnDia(participantes, dia);
        var solicitud = Solicitud.Crear(
            nombreMaterial,
            descripcion,
            tipoMaterial,
            prioridad,
            momento.Hoy.AddDays(plazoDias),
            solicitante,
            momento);

        return new CronologiaSolicitud(solicitud, solicitante, participantes, FotosEjemplo.MuestraPara(tipoMaterial));
    }

    public CronologiaSolicitud Asignar(Proveedor proveedor, int dia)
    {
        Solicitud.Asignar(proveedor, _participantes.Coordinador, EnDia(dia));
        return this;
    }

    public CronologiaSolicitud Declinar(string motivo, int dia)
    {
        Solicitud.Declinar(motivo, UsuarioProveedor, EnDia(dia));
        return this;
    }

    public CronologiaSolicitud Aceptar(int diaCompromiso, int dia)
    {
        Solicitud.Aceptar(Fecha(diaCompromiso), UsuarioProveedor, EnDia(dia));
        return this;
    }

    public CronologiaSolicitud CambiarFechaCompromiso(int diaCompromiso, int dia)
    {
        Solicitud.CambiarFechaCompromiso(Fecha(diaCompromiso), UsuarioProveedor, EnDia(dia));
        return this;
    }

    public CronologiaSolicitud Enviar(string transportista, string numeroGuia, int dia)
    {
        Solicitud.RegistrarEnvio(Fecha(dia), transportista, numeroGuia, UsuarioProveedor, EnDia(dia));
        return this;
    }

    public CronologiaSolicitud Evaluar(ResultadoEvaluacion resultado, string comentarios, int cantidadFotos, int dia)
    {
        var momento = EnDia(dia);
        var tamano = FotosEjemplo.Tamano(_muestraFoto);
        var fotos = Enumerable.Range(1, cantidadFotos)
            .Select(numero => FotoEvaluacion.Crear(
                FotosEjemplo.NombreArchivo(_muestraFoto),
                $"{_muestraFoto}-{numero}.png",
                FotosEjemplo.ContentType,
                tamano,
                momento.Utc))
            .ToList();

        Solicitud.RegistrarEvaluacion(resultado, comentarios, fotos, _participantes.Coordinador, momento);
        return this;
    }

    public CronologiaSolicitud Aprobar(int dia)
    {
        Solicitud.Aprobar(_solicitante, EnDia(dia));
        return this;
    }

    public CronologiaSolicitud RechazarEnAprobacion(string motivo, int dia)
    {
        Solicitud.Rechazar(motivo, _solicitante, EnDia(dia));
        return this;
    }

    public CronologiaSolicitud Autorizar(int dia)
    {
        Solicitud.Autorizar(_participantes.Autorizador, EnDia(dia));
        return this;
    }

    public CronologiaSolicitud RechazarEnAutorizacion(string motivo, int dia)
    {
        Solicitud.Rechazar(motivo, _participantes.Autorizador, EnDia(dia));
        return this;
    }

    private static Momento EnDia(ParticipantesEjemplo participantes, int dia) =>
        new(participantes.Ahora.Utc.AddDays(dia), participantes.Ahora.Hoy.AddDays(dia));

    private Momento EnDia(int dia) => EnDia(_participantes, dia);

    private DateOnly Fecha(int dia) => _participantes.Ahora.Hoy.AddDays(dia);
}
