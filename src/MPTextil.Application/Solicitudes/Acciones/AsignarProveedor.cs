using FluentValidation;

namespace MPTextil.Application.Solicitudes.Acciones;

public sealed record AsignarProveedor(int ProveedorId);

public sealed class ValidadorAsignarProveedor : AbstractValidator<AsignarProveedor>
{
    public ValidadorAsignarProveedor()
    {
        RuleFor(datos => datos.ProveedorId)
            .GreaterThan(0).WithMessage("Selecciona el proveedor.");
    }
}
