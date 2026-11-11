using MPTextil.Domain.Comun;
using MPTextil.Domain.Enumeraciones;

namespace MPTextil.Domain.Entidades;

public sealed class Decision : Entidad
{
    public const int MotivoMaximo = 500;

    private Decision()
    {
    }

    public int EvaluacionId { get; private set; }
    public NivelDecision Nivel { get; private set; }
    public bool Aprobada { get; private set; }
    public string? Motivo { get; private set; }
    public int UsuarioId { get; private set; }
    public DateTime Fecha { get; private set; }

    internal static Decision Aprobar(NivelDecision nivel, int usuarioId, DateTime fechaUtc) => new()
    {
        Nivel = nivel,
        Aprobada = true,
        UsuarioId = usuarioId,
        Fecha = fechaUtc
    };

    internal static Decision Rechazar(NivelDecision nivel, string motivo, int usuarioId, DateTime fechaUtc) => new()
    {
        Nivel = nivel,
        Aprobada = false,
        Motivo = Guardia.TextoObligatorio(motivo, "motivo del rechazo", MotivoMaximo),
        UsuarioId = usuarioId,
        Fecha = fechaUtc
    };
}
