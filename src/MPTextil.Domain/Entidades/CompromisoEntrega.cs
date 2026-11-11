using MPTextil.Domain.Comun;
using MPTextil.Domain.Enumeraciones;

namespace MPTextil.Domain.Entidades;

public sealed class CompromisoEntrega : Entidad
{
    private CompromisoEntrega()
    {
    }

    public int SolicitudId { get; private set; }
    public int ProveedorId { get; private set; }
    public DateOnly FechaCompromiso { get; private set; }
    public int RegistradoPorId { get; private set; }
    public DateTime FechaRegistro { get; private set; }
    public DateOnly? FechaReemplazo { get; private set; }
    public Envio? Envio { get; private set; }

    public bool EstaVigente => FechaReemplazo is null && Envio is null;

    public bool SeEntregoATiempo => Envio is not null && Envio.FechaEnvio <= FechaCompromiso;

    public bool TieneRechazo => Envio?.Evaluacion?.TieneRechazo ?? false;

    public bool EstaVencido(DateOnly hoy) => Envio is null && FechaCompromiso < (FechaReemplazo ?? hoy);

    internal Evaluacion EvaluacionRegistrada =>
        EnvioRegistrado.Evaluacion ?? throw new InvalidOperationException("El envío todavía no tiene una evaluación.");

    private Envio EnvioRegistrado =>
        Envio ?? throw new InvalidOperationException("El compromiso todavía no tiene un envío.");

    internal static CompromisoEntrega Crear(int proveedorId, DateOnly fechaCompromiso, int registradoPorId, DateTime fechaRegistroUtc) => new()
    {
        ProveedorId = proveedorId,
        FechaCompromiso = fechaCompromiso,
        RegistradoPorId = registradoPorId,
        FechaRegistro = fechaRegistroUtc
    };

    internal void Reemplazar(DateOnly fecha) => FechaReemplazo = fecha;

    internal Envio RegistrarEnvio(DateOnly fechaEnvio, string transportista, string numeroGuia, int registradoPorId, DateTime fechaRegistroUtc)
    {
        if (!EstaVigente)
        {
            throw new InvalidOperationException("El compromiso ya no está vigente.");
        }

        Envio = Envio.Crear(fechaEnvio, transportista, numeroGuia, registradoPorId, fechaRegistroUtc);
        return Envio;
    }

    internal Evaluacion RegistrarEvaluacion(
        ResultadoEvaluacion resultado,
        string comentarios,
        IReadOnlyCollection<FotoEvaluacion> fotos,
        int evaluadoPorId,
        DateTime fechaUtc) =>
        EnvioRegistrado.Evaluar(resultado, comentarios, fotos, evaluadoPorId, fechaUtc);
}
