using FluentValidation;
using MPTextil.Application.Abstracciones.Tiempo;

namespace MPTextil.Application.Solicitudes.Acciones;

public sealed record FechaCompromisoSolicitud(DateOnly FechaCompromiso);

public sealed class ValidadorFechaCompromisoSolicitud : AbstractValidator<FechaCompromisoSolicitud>
{
    public ValidadorFechaCompromisoSolicitud(IReloj reloj)
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(datos => datos.FechaCompromiso)
            .NotEmpty().WithMessage("Indica la fecha compromiso.")
            .GreaterThanOrEqualTo(_ => reloj.Ahora().Hoy).WithMessage("La fecha compromiso no puede ser anterior a hoy.");
    }
}
