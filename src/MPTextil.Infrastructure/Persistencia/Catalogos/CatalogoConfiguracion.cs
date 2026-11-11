using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MPTextil.Domain.Enumeraciones;

namespace MPTextil.Infrastructure.Persistencia.Catalogos;

internal abstract class CatalogoConfiguracion<TEnum>(string tabla, Func<TEnum, string> describir)
    : IEntityTypeConfiguration<Catalogo<TEnum>> where TEnum : struct, Enum
{
    private const int NombreMaximo = 50;

    public void Configure(EntityTypeBuilder<Catalogo<TEnum>> builder)
    {
        builder.ToTable(tabla);
        builder.HasKey(catalogo => catalogo.Id);
        builder.Property(catalogo => catalogo.Id).ValueGeneratedNever();
        builder.Property(catalogo => catalogo.Nombre).HasMaxLength(NombreMaximo);
        builder.HasIndex(catalogo => catalogo.Nombre).IsUnique();

        builder.HasData(Enum.GetValues<TEnum>().Select(valor => new Catalogo<TEnum> { Id = valor, Nombre = describir(valor) }));
    }
}

internal sealed class RolConfiguracion()
    : CatalogoConfiguracion<Rol>("Rol", rol => rol.Descripcion());

internal sealed class EstadoSolicitudConfiguracion()
    : CatalogoConfiguracion<EstadoSolicitud>("EstadoSolicitud", estado => estado.Descripcion());

internal sealed class PrioridadConfiguracion()
    : CatalogoConfiguracion<Prioridad>("Prioridad", prioridad => prioridad.Descripcion());
