using MPTextil.Domain.Comun;
using MPTextil.Domain.Enumeraciones;

namespace MPTextil.Domain.Entidades;

public sealed class Envio : Entidad
{
    public const int TransportistaMaximo = 100;
    public const int NumeroGuiaMaximo = 50;

    private Envio()
    {
    }

    public int CompromisoId { get; private set; }
    public DateOnly FechaEnvio { get; private set; }
    public string Transportista { get; private set; } = string.Empty;
    public string NumeroGuia { get; private set; } = string.Empty;
    public int RegistradoPorId { get; private set; }
    public DateTime FechaRegistro { get; private set; }
    public Evaluacion? Evaluacion { get; private set; }

    internal static Envio Crear(DateOnly fechaEnvio, string transportista, string numeroGuia, int registradoPorId, DateTime fechaRegistroUtc) => new()
    {
        FechaEnvio = fechaEnvio,
        Transportista = Guardia.TextoObligatorio(transportista, "transportista", TransportistaMaximo),
        NumeroGuia = Guardia.TextoObligatorio(numeroGuia, "número de guía", NumeroGuiaMaximo),
        RegistradoPorId = registradoPorId,
        FechaRegistro = fechaRegistroUtc
    };

    internal Evaluacion Evaluar(
        ResultadoEvaluacion resultado,
        string comentarios,
        IReadOnlyCollection<FotoEvaluacion> fotos,
        int evaluadoPorId,
        DateTime fechaUtc)
    {
        if (Evaluacion is not null)
        {
            throw new InvalidOperationException("El envío ya fue evaluado.");
        }

        Evaluacion = Evaluacion.Crear(resultado, comentarios, fotos, evaluadoPorId, fechaUtc);
        return Evaluacion;
    }
}
