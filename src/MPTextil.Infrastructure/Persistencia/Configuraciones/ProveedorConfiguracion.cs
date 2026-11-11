using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MPTextil.Domain.Entidades;

namespace MPTextil.Infrastructure.Persistencia.Configuraciones;

internal sealed class ProveedorConfiguracion : IEntityTypeConfiguration<Proveedor>
{
    public void Configure(EntityTypeBuilder<Proveedor> builder)
    {
        builder.ToTable("Proveedor");
        builder.HasKey(proveedor => proveedor.Id);
        builder.Property(proveedor => proveedor.Nombre).HasMaxLength(Proveedor.NombreMaximo);
        builder.Property(proveedor => proveedor.Correo).HasMaxLength(Proveedor.CorreoMaximo);
        builder.Property(proveedor => proveedor.Telefono).HasMaxLength(Proveedor.TelefonoMaximo);
        builder.HasIndex(proveedor => proveedor.Nombre).IsUnique();
    }
}
