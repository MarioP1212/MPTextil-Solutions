using MPTextil.Domain.Comun;
using MPTextil.Domain.Enumeraciones;
using MPTextil.Domain.Excepciones;
using MPTextil.Domain.Flujo;

namespace MPTextil.Domain.Entidades;

public sealed class Solicitud : Entidad
{
    public const int CodigoMaximo = 20;
    public const int NombreMaterialMaximo = 150;
    public const int DescripcionMaxima = 1000;
    public const int MotivoMaximo = 500;

    private readonly List<CompromisoEntrega> _compromisos = [];
    private readonly List<HistorialSolicitud> _historial = [];

    private Solicitud()
    {
    }

    public string Codigo { get; private set; } = string.Empty;
    public string NombreMaterial { get; private set; } = string.Empty;
    public string Descripcion { get; private set; } = string.Empty;
    public int TipoMaterialId { get; private set; }
    public TipoMaterial? TipoMaterial { get; private set; }
    public Prioridad Prioridad { get; private set; }
    public EstadoSolicitud Estado { get; private set; }
    public int SolicitanteId { get; private set; }
    public Usuario? Solicitante { get; private set; }
    public int? ProveedorId { get; private set; }
    public Proveedor? Proveedor { get; private set; }
    public DateOnly FechaObjetivo { get; private set; }
    public DateTime FechaCreacion { get; private set; }
    public DateTime FechaActualizacion { get; private set; }

    public IReadOnlyCollection<CompromisoEntrega> Compromisos => _compromisos.AsReadOnly();
    public IReadOnlyCollection<HistorialSolicitud> Historial => _historial.AsReadOnly();

    public CompromisoEntrega? CompromisoVigente => _compromisos.SingleOrDefault(compromiso => compromiso.EstaVigente);

    public bool RequiereNuevaFechaCompromiso => Estado == EstadoSolicitud.EnProgreso && CompromisoVigente is null;

    public static bool EsVencida(EstadoSolicitud estado, DateOnly? fechaCompromisoVigente, DateOnly hoy) =>
        estado == EstadoSolicitud.EnProgreso && fechaCompromisoVigente < hoy;

    public bool EstaVencida(DateOnly hoy) => EsVencida(Estado, CompromisoVigente?.FechaCompromiso, hoy);

    private bool TieneRechazos => _compromisos.Any(compromiso => compromiso.TieneRechazo);

    private int ProveedorAsignadoId =>
        ProveedorId ?? throw new InvalidOperationException("La solicitud no tiene un proveedor asignado.");

    private CompromisoEntrega UltimoCompromisoEnviado =>
        _compromisos
            .Where(compromiso => compromiso.Envio is not null)
            .MaxBy(compromiso => (compromiso.FechaRegistro, compromiso.Id))
        ?? throw new InvalidOperationException("La solicitud no tiene envíos registrados.");

    public static Solicitud Crear(
        string nombreMaterial,
        string descripcion,
        TipoMaterial tipoMaterial,
        Prioridad prioridad,
        DateOnly fechaObjetivo,
        Actor actor,
        Momento momento)
    {
        if (actor.Rol != Rol.Solicitante)
        {
            throw new AccesoDenegadoException("Solo un usuario con rol Solicitante puede crear solicitudes.");
        }

        Guardia.Validar(tipoMaterial.Activo, "El tipo de material seleccionado no está disponible.");
        Guardia.Validar(fechaObjetivo >= momento.Hoy, "La fecha objetivo no puede ser anterior a hoy.");

        var solicitud = new Solicitud
        {
            NombreMaterial = Guardia.TextoObligatorio(nombreMaterial, "nombre del material", NombreMaterialMaximo),
            Descripcion = Guardia.TextoObligatorio(descripcion, "descripción", DescripcionMaxima),
            TipoMaterialId = tipoMaterial.Id,
            Prioridad = Guardia.Definido(prioridad, "prioridad"),
            Estado = EstadoSolicitud.Solicitado,
            SolicitanteId = actor.UsuarioId,
            FechaObjetivo = fechaObjetivo,
            FechaCreacion = momento.Utc,
            FechaActualizacion = momento.Utc
        };

        solicitud.RegistrarHistorial(
            AccionSolicitud.Crear,
            null,
            actor,
            $"Solicitud de {tipoMaterial.Nombre} con prioridad {prioridad.Descripcion()} para el {fechaObjetivo.ATexto()}.",
            momento);

        return solicitud;
    }

    public void Asignar(Proveedor proveedor, Actor actor, Momento momento)
    {
        var transicion = ValidarAccion(AccionSolicitud.Asignar, actor);
        Guardia.Validar(proveedor.Activo, "El proveedor seleccionado no está activo.");

        ProveedorId = proveedor.Id;
        Avanzar(transicion, actor, $"Asignada al proveedor {proveedor.Nombre}.", momento);
    }

    public void Declinar(string motivo, Actor actor, Momento momento)
    {
        var transicion = ValidarAccion(AccionSolicitud.Declinar, actor);
        var motivoValidado = Guardia.TextoObligatorio(motivo, "motivo", MotivoMaximo);

        ProveedorId = null;
        Avanzar(transicion, actor, $"El proveedor declinó la asignación. Motivo: {motivoValidado}", momento);
    }

    public void Aceptar(DateOnly fechaCompromiso, Actor actor, Momento momento)
    {
        var transicion = ValidarAccion(AccionSolicitud.Aceptar, actor);
        ValidarFechaCompromiso(fechaCompromiso, momento);

        AgregarCompromiso(fechaCompromiso, actor, momento);
        Avanzar(transicion, actor, $"Aceptada con fecha compromiso {fechaCompromiso.ATexto()}.", momento);
    }

    public void CambiarFechaCompromiso(DateOnly nuevaFecha, Actor actor, Momento momento)
    {
        var transicion = ValidarAccion(AccionSolicitud.CambiarFechaCompromiso, actor);
        Guardia.Validar(TieneRechazos, "Solo puedes cambiar la fecha compromiso después de que la muestra fue rechazada.");
        ValidarFechaCompromiso(nuevaFecha, momento);

        var vigente = CompromisoVigente;
        Guardia.Validar(vigente?.FechaCompromiso != nuevaFecha, "La nueva fecha compromiso es igual a la vigente.");

        vigente?.Reemplazar(momento.Hoy);
        AgregarCompromiso(nuevaFecha, actor, momento);

        var detalle = vigente is null
            ? $"Nueva fecha compromiso {nuevaFecha.ATexto()}."
            : $"Fecha compromiso cambiada del {vigente.FechaCompromiso.ATexto()} al {nuevaFecha.ATexto()}.";

        Avanzar(transicion, actor, detalle, momento);
    }

    public void RegistrarEnvio(DateOnly fechaEnvio, string transportista, string numeroGuia, Actor actor, Momento momento)
    {
        var transicion = ValidarAccion(AccionSolicitud.RegistrarEnvio, actor);
        var vigente = CompromisoVigente
            ?? throw new ReglaNegocioException("Registra una nueva fecha compromiso antes de enviar la muestra.");
        Guardia.Validar(fechaEnvio <= momento.Hoy, "La fecha de envío no puede ser posterior a hoy.");

        var envio = vigente.RegistrarEnvio(fechaEnvio, transportista, numeroGuia, actor.UsuarioId, momento.Utc);
        Avanzar(
            transicion,
            actor,
            $"Enviada el {envio.FechaEnvio.ATexto()} con {envio.Transportista}, guía {envio.NumeroGuia}.",
            momento);
    }

    public void RegistrarEvaluacion(
        ResultadoEvaluacion resultado,
        string comentarios,
        IReadOnlyCollection<FotoEvaluacion> fotos,
        Actor actor,
        Momento momento)
    {
        var transicion = ValidarAccion(AccionSolicitud.RegistrarEvaluacion, actor);

        var evaluacion = UltimoCompromisoEnviado.RegistrarEvaluacion(resultado, comentarios, fotos, actor.UsuarioId, momento.Utc);
        Avanzar(
            transicion,
            actor,
            $"Resultado: {evaluacion.Resultado.Descripcion()}. Fotos adjuntas: {evaluacion.Fotos.Count}.",
            momento);
    }

    public void Aprobar(Actor actor, Momento momento)
    {
        var transicion = ValidarAccion(AccionSolicitud.Aprobar, actor);

        UltimoCompromisoEnviado.EvaluacionRegistrada.RegistrarAprobacion(NivelDecision.Aprobacion, actor.UsuarioId, momento.Utc);
        Avanzar(transicion, actor, "Muestra aprobada por el solicitante. Queda pendiente de autorización.", momento);
    }

    public void Autorizar(Actor actor, Momento momento)
    {
        var transicion = ValidarAccion(AccionSolicitud.Autorizar, actor);

        UltimoCompromisoEnviado.EvaluacionRegistrada.RegistrarAprobacion(NivelDecision.Autorizacion, actor.UsuarioId, momento.Utc);
        Avanzar(transicion, actor, "Muestra autorizada. La solicitud queda cerrada.", momento);
    }

    public void Rechazar(string motivo, Actor actor, Momento momento)
    {
        var transicion = ValidarAccion(AccionSolicitud.Rechazar, actor);
        var nivel = transicion.Origen == EstadoSolicitud.EnEvaluacion ? NivelDecision.Aprobacion : NivelDecision.Autorizacion;
        var compromisoEvaluado = UltimoCompromisoEnviado;

        var decision = compromisoEvaluado.EvaluacionRegistrada.RegistrarRechazo(nivel, motivo, actor.UsuarioId, momento.Utc);
        var continuidad = ReabrirCompromiso(compromisoEvaluado.FechaCompromiso, actor, momento);

        Avanzar(
            transicion,
            actor,
            $"Muestra rechazada en {nivel.Descripcion().ToLowerInvariant()}. Motivo: {decision.Motivo?.TrimEnd('.')}. {continuidad}",
            momento);
    }

    public IReadOnlyList<AccionSolicitud> AccionesDisponibles(Actor actor)
    {
        if (!PerteneceA(actor))
        {
            return [];
        }

        return FlujoSolicitud.DisponiblesDesde(Estado, actor.Rol)
            .Select(transicion => transicion.Accion)
            .Where(accion => accion != AccionSolicitud.CambiarFechaCompromiso || TieneRechazos)
            .Where(accion => accion != AccionSolicitud.RegistrarEnvio || CompromisoVigente is not null)
            .Distinct()
            .ToList();
    }

    public Transicion ValidarAccion(AccionSolicitud accion, Actor actor)
    {
        if (!FlujoSolicitud.RolPuedeEjecutar(actor.Rol, accion))
        {
            throw new AccesoDenegadoException($"Tu rol de {actor.Rol.Descripcion()} no permite {accion.Descripcion()}.");
        }

        if (!PerteneceA(actor))
        {
            throw new AccesoDenegadoException(actor.Rol == Rol.Proveedor
                ? "Esta solicitud no está asignada a tu empresa."
                : "Solo el solicitante que creó la solicitud puede realizar esta acción.");
        }

        return FlujoSolicitud.Buscar(Estado, accion, actor.Rol)
            ?? throw new TransicionInvalidaException(accion, Estado);
    }

    private bool PerteneceA(Actor actor) => actor.Rol switch
    {
        Rol.Proveedor => actor.ProveedorId is not null && actor.ProveedorId == ProveedorId,
        Rol.Solicitante => actor.UsuarioId == SolicitanteId,
        _ => true
    };

    private static void ValidarFechaCompromiso(DateOnly fechaCompromiso, Momento momento) =>
        Guardia.Validar(fechaCompromiso >= momento.Hoy, "La fecha compromiso no puede ser anterior a hoy.");

    private string ReabrirCompromiso(DateOnly fechaAnterior, Actor actor, Momento momento)
    {
        if (fechaAnterior < momento.Hoy)
        {
            return "La fecha compromiso ya pasó, el proveedor debe registrar una nueva.";
        }

        AgregarCompromiso(fechaAnterior, actor, momento);
        return $"Se mantiene la fecha compromiso {fechaAnterior.ATexto()}.";
    }

    private void AgregarCompromiso(DateOnly fechaCompromiso, Actor actor, Momento momento) =>
        _compromisos.Add(CompromisoEntrega.Crear(ProveedorAsignadoId, fechaCompromiso, actor.UsuarioId, momento.Utc));

    private void Avanzar(Transicion transicion, Actor actor, string detalle, Momento momento)
    {
        Estado = transicion.Destino;
        FechaActualizacion = momento.Utc;
        RegistrarHistorial(transicion.Accion, transicion.Origen, actor, detalle, momento);
    }

    private void RegistrarHistorial(AccionSolicitud accion, EstadoSolicitud? estadoAnterior, Actor actor, string detalle, Momento momento) =>
        _historial.Add(HistorialSolicitud.Registrar(accion, estadoAnterior, Estado, actor.UsuarioId, detalle, momento.Utc));
}
