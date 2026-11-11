using MPTextil.Domain.Comun;
using MPTextil.Domain.Enumeraciones;

namespace MPTextil.Domain.Entidades;

public sealed class Evaluacion : Entidad
{
    public const int ComentariosMaximo = 1000;
    public const int MinimoFotos = 1;
    public const int MaximoFotos = 5;

    private readonly List<FotoEvaluacion> _fotos = [];
    private readonly List<Decision> _decisiones = [];

    private Evaluacion()
    {
    }

    public int EnvioId { get; private set; }
    public ResultadoEvaluacion Resultado { get; private set; }
    public string Comentarios { get; private set; } = string.Empty;
    public int EvaluadoPorId { get; private set; }
    public DateTime FechaEvaluacion { get; private set; }

    public IReadOnlyCollection<FotoEvaluacion> Fotos => _fotos.AsReadOnly();
    public IReadOnlyCollection<Decision> Decisiones => _decisiones.AsReadOnly();

    public bool TieneRechazo => _decisiones.Any(decision => !decision.Aprobada);

    internal static Evaluacion Crear(
        ResultadoEvaluacion resultado,
        string comentarios,
        IReadOnlyCollection<FotoEvaluacion> fotos,
        int evaluadoPorId,
        DateTime fechaUtc)
    {
        Guardia.Validar(
            fotos.Count is >= MinimoFotos and <= MaximoFotos,
            $"Adjunta entre {MinimoFotos} y {MaximoFotos} fotos de la muestra.");

        var evaluacion = new Evaluacion
        {
            Resultado = Guardia.Definido(resultado, "resultado"),
            Comentarios = Guardia.TextoObligatorio(comentarios, "comentarios", ComentariosMaximo),
            EvaluadoPorId = evaluadoPorId,
            FechaEvaluacion = fechaUtc
        };

        evaluacion._fotos.AddRange(fotos);
        return evaluacion;
    }

    internal void RegistrarAprobacion(NivelDecision nivel, int usuarioId, DateTime fechaUtc) =>
        AgregarDecision(Decision.Aprobar(nivel, usuarioId, fechaUtc));

    internal Decision RegistrarRechazo(NivelDecision nivel, string motivo, int usuarioId, DateTime fechaUtc)
    {
        var decision = Decision.Rechazar(nivel, motivo, usuarioId, fechaUtc);
        AgregarDecision(decision);
        return decision;
    }

    private void AgregarDecision(Decision decision)
    {
        if (_decisiones.Any(existente => existente.Nivel == decision.Nivel))
        {
            throw new InvalidOperationException($"La evaluación ya tiene una decisión de {decision.Nivel.Descripcion()}.");
        }

        _decisiones.Add(decision);
    }
}
