using FluentValidation;
using MPTextil.Domain.Entidades;

namespace MPTextil.Application.Solicitudes.Acciones;

public sealed record MotivoAccion(string Motivo);

public sealed class ValidadorMotivoAccion : AbstractValidator<MotivoAccion>
{
    public ValidadorMotivoAccion()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(datos => datos.Motivo)
            .NotEmpty().WithMessage("Escribe el motivo.")
            .MaximumLength(Solicitud.MotivoMaximo)
            .WithMessage($"El motivo no puede superar {Solicitud.MotivoMaximo} caracteres.");
    }
}
