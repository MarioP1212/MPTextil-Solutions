using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MPTextil.Domain.Entidades;
using MPTextil.Domain.Enumeraciones;
using MPTextil.Infrastructure.Persistencia.Catalogos;

namespace MPTextil.Infrastructure.Persistencia.Configuraciones;

internal sealed class UsuarioConfiguracion : IEntityTypeConfiguration<Usuario>
{
    private static readonly int RolProveedor = (int)Rol.Proveedor;

    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("Usuario", tabla => tabla.HasCheckConstraint(
            "CK_Usuario_ProveedorSegunRol",
            $"([RolId] = {RolProveedor} AND [ProveedorId] IS NOT NULL) OR ([RolId] <> {RolProveedor} AND [ProveedorId] IS NULL)"));

        builder.HasKey(usuario => usuario.Id);
        builder.Property(usuario => usuario.Nombre).HasMaxLength(Usuario.NombreMaximo);
        builder.Property(usuario => usuario.Correo).HasMaxLength(Usuario.CorreoMaximo);
        builder.Property(usuario => usuario.PasswordHash).HasMaxLength(Usuario.PasswordHashMaximo);
        builder.Property(usuario => usuario.Rol).HasColumnName("RolId");
        builder.HasIndex(usuario => usuario.Correo).IsUnique();

        builder.HasOne<Catalogo<Rol>>()
            .WithMany()
            .HasForeignKey(usuario => usuario.Rol)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(usuario => usuario.Proveedor)
            .WithMany()
            .HasForeignKey(usuario => usuario.ProveedorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
