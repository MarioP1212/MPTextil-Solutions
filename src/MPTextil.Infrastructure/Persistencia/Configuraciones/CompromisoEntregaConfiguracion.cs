using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MPTextil.Domain.Entidades;

namespace MPTextil.Infrastructure.Persistencia.Configuraciones;

internal sealed class CompromisoEntregaConfiguracion : IEntityTypeConfiguration<CompromisoEntrega>
{
    public void Configure(EntityTypeBuilder<CompromisoEntrega> builder)
    {
        builder.ToTable("CompromisoEntrega");
        builder.HasKey(compromiso => compromiso.Id);
        builder.HasIndex(compromiso => new { compromiso.ProveedorId, compromiso.FechaCompromiso });

        builder.Ignore(compromiso => compromiso.EstaVigente);
        builder.Ignore(compromiso => compromiso.SeEntregoATiempo);
        builder.Ignore(compromiso => compromiso.TieneRechazo);

        builder.HasOne<Proveedor>()
            .WithMany()
            .HasForeignKey(compromiso => compromiso.ProveedorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(compromiso => compromiso.RegistradoPorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(compromiso => compromiso.Envio)
            .WithOne()
            .HasForeignKey<Envio>(envio => envio.CompromisoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
