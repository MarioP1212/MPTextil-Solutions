using FluentValidation;
using MPTextil.Application.Abstracciones.Tiempo;
using MPTextil.Domain.Entidades;

namespace MPTextil.Application.Solicitudes.Acciones;

public sealed record RegistrarEnvioSolicitud(DateOnly FechaEnvio, string Transportista, string NumeroGuia);

public sealed class ValidadorRegistrarEnvioSolicitud : AbstractValidator<RegistrarEnvioSolicitud>
{
    public ValidadorRegistrarEnvioSolicitud(IReloj reloj)
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(datos => datos.FechaEnvio)
            .NotEmpty().WithMessage("Indica la fecha de envío.")
            .LessThanOrEqualTo(_ => reloj.Ahora().Hoy).WithMessage("La fecha de envío no puede ser posterior a hoy.");

        RuleFor(datos => datos.Transportista)
            .NotEmpty().WithMessage("Escribe el nombre del transportista.")
            .MaximumLength(Envio.TransportistaMaximo)
            .WithMessage($"El transportista no puede superar {Envio.TransportistaMaximo} caracteres.");

        RuleFor(datos => datos.NumeroGuia)
            .NotEmpty().WithMessage("Escribe el número de guía.")
            .MaximumLength(Envio.NumeroGuiaMaximo)
            .WithMessage($"El número de guía no puede superar {Envio.NumeroGuiaMaximo} caracteres.");
    }
}
