using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MPTextil.Domain.Entidades;
using MPTextil.Domain.Enumeraciones;
using MPTextil.Infrastructure.Persistencia.Catalogos;

namespace MPTextil.Infrastructure.Persistencia.Configuraciones;

internal sealed class HistorialSolicitudConfiguracion : IEntityTypeConfiguration<HistorialSolicitud>
{
    private const int AccionMaxima = 40;

    public void Configure(EntityTypeBuilder<HistorialSolicitud> builder)
    {
        builder.ToTable("HistorialSolicitud");
        builder.HasKey(registro => registro.Id);
        builder.Property(registro => registro.Accion).HasConversion<string>().HasMaxLength(AccionMaxima);
        builder.Property(registro => registro.Detalle).HasMaxLength(HistorialSolicitud.DetalleMaximo);
        builder.Property(registro => registro.EstadoAnterior).HasColumnName("EstadoAnteriorId");
        builder.Property(registro => registro.EstadoNuevo).HasColumnName("EstadoNuevoId");
        builder.HasIndex(registro => new { registro.SolicitudId, registro.Fecha });

        builder.HasOne(registro => registro.Usuario)
            .WithMany()
            .HasForeignKey(registro => registro.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Catalogo<EstadoSolicitud>>()
            .WithMany()
            .HasForeignKey(registro => registro.EstadoAnterior)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Catalogo<EstadoSolicitud>>()
            .WithMany()
            .HasForeignKey(registro => registro.EstadoNuevo)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
