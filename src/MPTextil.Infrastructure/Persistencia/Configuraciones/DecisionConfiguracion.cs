using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MPTextil.Domain.Entidades;
using MPTextil.Domain.Enumeraciones;

namespace MPTextil.Infrastructure.Persistencia.Configuraciones;

internal sealed class DecisionConfiguracion : IEntityTypeConfiguration<Decision>
{
    private static readonly string NivelesValidos =
        string.Join(", ", Enum.GetValues<NivelDecision>().Select(nivel => (int)nivel));

    public void Configure(EntityTypeBuilder<Decision> builder)
    {
        builder.ToTable("Decision", tabla =>
        {
            tabla.HasCheckConstraint("CK_Decision_Nivel", $"[Nivel] IN ({NivelesValidos})");
            tabla.HasCheckConstraint("CK_Decision_MotivoAlRechazar", "[Aprobada] = 1 OR [Motivo] IS NOT NULL");
        });

        builder.HasKey(decision => decision.Id);
        builder.Property(decision => decision.Motivo).HasMaxLength(Decision.MotivoMaximo);
        builder.HasIndex(decision => new { decision.EvaluacionId, decision.Nivel })
            .IsUnique()
            .HasDatabaseName(IndicesConcurrencia.DecisionPorNivel);

        builder.HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(decision => decision.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
