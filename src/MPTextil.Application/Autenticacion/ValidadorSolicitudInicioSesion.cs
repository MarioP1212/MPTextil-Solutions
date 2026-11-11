using FluentValidation;

namespace MPTextil.Application.Autenticacion;

public sealed class ValidadorSolicitudInicioSesion : AbstractValidator<SolicitudInicioSesion>
{
    public ValidadorSolicitudInicioSesion()
    {
        RuleFor(solicitud => solicitud.Correo)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Ingresa tu correo.")
            .EmailAddress().WithMessage("Ingresa un correo con un formato válido, por ejemplo nombre@empresa.com.");

        RuleFor(solicitud => solicitud.Contrasena)
            .NotEmpty().WithMessage("Ingresa tu contraseña.");
    }
}
