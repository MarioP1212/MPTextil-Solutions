namespace MPTextil.Infrastructure.Persistencia;

internal static class IndicesConcurrencia
{
    public const string DecisionPorNivel = "IX_Decision_EvaluacionId_Nivel";
    public const string EnvioPorCompromiso = "IX_Envio_CompromisoId";
    public const string EvaluacionPorEnvio = "IX_Evaluacion_EnvioId";

    public static readonly string[] Todos = [DecisionPorNivel, EnvioPorCompromiso, EvaluacionPorEnvio];
}
