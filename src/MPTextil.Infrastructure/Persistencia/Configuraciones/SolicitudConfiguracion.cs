using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MPTextil.Domain.Entidades;
using MPTextil.Domain.Enumeraciones;
using MPTextil.Infrastructure.Persistencia.Catalogos;

namespace MPTextil.Infrastructure.Persistencia.Configuraciones;

internal sealed class SolicitudConfiguracion : IEntityTypeConfiguration<Solicitud>
{
    private const string ColumnaVersion = "Version";

    private const string CodigoCalculado =
        "'SOL-' + CASE WHEN [Id] < 100000 THEN RIGHT('00000' + CAST([Id] AS varchar(10)), 5) ELSE CAST([Id] AS varchar(10)) END";

    public void Configure(EntityTypeBuilder<Solicitud> builder)
    {
        builder.ToTable("Solicitud");
        builder.HasKey(solicitud => solicitud.Id);

        builder.Property(solicitud => solicitud.Codigo)
            .HasMaxLength(Solicitud.CodigoMaximo)
            .HasComputedColumnSql(CodigoCalculado, stored: true);

        builder.Property(solicitud => solicitud.NombreMaterial).HasMaxLength(Solicitud.NombreMaterialMaximo);
        builder.Property(solicitud => solicitud.Descripcion).HasMaxLength(Solicitud.DescripcionMaxima);
        builder.Property(solicitud => solicitud.Prioridad).HasColumnName("PrioridadId");
        builder.Property(solicitud => solicitud.Estado).HasColumnName("EstadoId");
        builder.Property<byte[]>(ColumnaVersion).IsRowVersion();

        builder.HasIndex(solicitud => solicitud.Codigo).IsUnique();
        builder.HasIndex(solicitud => solicitud.Estado);

        builder.Ignore(solicitud => solicitud.CompromisoVigente);
        builder.Ignore(solicitud => solicitud.RequiereNuevaFechaCompromiso);

        builder.HasOne(solicitud => solicitud.TipoMaterial)
            .WithMany()
            .HasForeignKey(solicitud => solicitud.TipoMaterialId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(solicitud => solicitud.Solicitante)
            .WithMany()
            .HasForeignKey(solicitud => solicitud.SolicitanteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(solicitud => solicitud.Proveedor)
            .WithMany()
            .HasForeignKey(solicitud => solicitud.ProveedorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Catalogo<Prioridad>>()
            .WithMany()
            .HasForeignKey(solicitud => solicitud.Prioridad)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Catalogo<EstadoSolicitud>>()
            .WithMany()
            .HasForeignKey(solicitud => solicitud.Estado)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(solicitud => solicitud.Compromisos)
            .WithOne()
            .HasForeignKey(compromiso => compromiso.SolicitudId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(solicitud => solicitud.Historial)
            .WithOne()
            .HasForeignKey(registro => registro.SolicitudId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(solicitud => solicitud.Compromisos).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(solicitud => solicitud.Historial).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
