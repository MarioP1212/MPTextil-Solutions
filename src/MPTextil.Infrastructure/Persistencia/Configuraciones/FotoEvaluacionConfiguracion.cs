using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MPTextil.Domain.Entidades;

namespace MPTextil.Infrastructure.Persistencia.Configuraciones;

internal sealed class FotoEvaluacionConfiguracion : IEntityTypeConfiguration<FotoEvaluacion>
{
    public void Configure(EntityTypeBuilder<FotoEvaluacion> builder)
    {
        builder.ToTable("FotoEvaluacion", tabla => tabla.HasCheckConstraint(
            "CK_FotoEvaluacion_Tamano",
            $"[TamanoBytes] > 0 AND [TamanoBytes] <= {FotoEvaluacion.TamanoMaximoBytes}"));

        builder.HasKey(foto => foto.Id);
        builder.Property(foto => foto.BlobNombre).HasMaxLength(FotoEvaluacion.BlobNombreMaximo);
        builder.Property(foto => foto.NombreOriginal).HasMaxLength(FotoEvaluacion.NombreOriginalMaximo);
        builder.Property(foto => foto.ContentType).HasMaxLength(FotoEvaluacion.ContentTypeMaximo);
        builder.HasIndex(foto => foto.BlobNombre).IsUnique();
    }
}
