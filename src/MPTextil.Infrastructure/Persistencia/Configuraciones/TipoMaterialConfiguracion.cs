using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MPTextil.Domain.Entidades;

namespace MPTextil.Infrastructure.Persistencia.Configuraciones;

internal sealed class TipoMaterialConfiguracion : IEntityTypeConfiguration<TipoMaterial>
{
    public void Configure(EntityTypeBuilder<TipoMaterial> builder)
    {
        builder.ToTable("TipoMaterial");
        builder.HasKey(tipo => tipo.Id);
        builder.Property(tipo => tipo.Nombre).HasMaxLength(TipoMaterial.NombreMaximo);
        builder.HasIndex(tipo => tipo.Nombre).IsUnique();

        builder.HasData(
            new { Id = 1, Nombre = "Tela", Activo = true },
            new { Id = 2, Nombre = "Malla", Activo = true },
            new { Id = 3, Nombre = "Etiqueta", Activo = true },
            new { Id = 4, Nombre = "Hilo", Activo = true });
    }
}
