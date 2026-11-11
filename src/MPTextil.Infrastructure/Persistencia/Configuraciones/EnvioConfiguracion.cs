using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MPTextil.Domain.Entidades;

namespace MPTextil.Infrastructure.Persistencia.Configuraciones;

internal sealed class EnvioConfiguracion : IEntityTypeConfiguration<Envio>
{
    public void Configure(EntityTypeBuilder<Envio> builder)
    {
        builder.ToTable("Envio");
        builder.HasKey(envio => envio.Id);
        builder.Property(envio => envio.Transportista).HasMaxLength(Envio.TransportistaMaximo);
        builder.Property(envio => envio.NumeroGuia).HasMaxLength(Envio.NumeroGuiaMaximo);
        builder.HasIndex(envio => envio.CompromisoId)
            .IsUnique()
            .HasDatabaseName(IndicesConcurrencia.EnvioPorCompromiso);

        builder.HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(envio => envio.RegistradoPorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(envio => envio.Evaluacion)
            .WithOne()
            .HasForeignKey<Evaluacion>(evaluacion => evaluacion.EnvioId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
