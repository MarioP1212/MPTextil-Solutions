using Microsoft.EntityFrameworkCore;
using MPTextil.Domain.Entidades;

namespace MPTextil.Infrastructure.Persistencia;

public sealed class MPTextilDbContext(DbContextOptions<MPTextilDbContext> opciones) : DbContext(opciones)
{
    public DbSet<Solicitud> Solicitudes => Set<Solicitud>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Proveedor> Proveedores => Set<Proveedor>();
    public DbSet<TipoMaterial> TiposMaterial => Set<TipoMaterial>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MPTextilDbContext).Assembly);
}
