using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MPTextil.Domain.Entidades;
using MPTextil.Domain.Enumeraciones;

namespace MPTextil.Infrastructure.Persistencia.Configuraciones;

internal sealed class EvaluacionConfiguracion : IEntityTypeConfiguration<Evaluacion>
{
    private static readonly string ResultadosValidos =
        string.Join(", ", Enum.GetValues<ResultadoEvaluacion>().Select(resultado => (int)resultado));

    public void Configure(EntityTypeBuilder<Evaluacion> builder)
    {
        builder.ToTable("Evaluacion", tabla => tabla.HasCheckConstraint(
            "CK_Evaluacion_Resultado",
            $"[Resultado] IN ({ResultadosValidos})"));

        builder.HasKey(evaluacion => evaluacion.Id);
        builder.Property(evaluacion => evaluacion.Comentarios).HasMaxLength(Evaluacion.ComentariosMaximo);
        builder.Ignore(evaluacion => evaluacion.TieneRechazo);
        builder.HasIndex(evaluacion => evaluacion.EnvioId)
            .IsUnique()
            .HasDatabaseName(IndicesConcurrencia.EvaluacionPorEnvio);

        builder.HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(evaluacion => evaluacion.EvaluadoPorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(evaluacion => evaluacion.Fotos)
            .WithOne()
            .HasForeignKey(foto => foto.EvaluacionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(evaluacion => evaluacion.Decisiones)
            .WithOne()
            .HasForeignKey(decision => decision.EvaluacionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(evaluacion => evaluacion.Fotos).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(evaluacion => evaluacion.Decisiones).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
