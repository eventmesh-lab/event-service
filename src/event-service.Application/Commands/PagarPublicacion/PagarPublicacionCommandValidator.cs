using FluentValidation;

namespace event_service.Application.Commands.PagarPublicacion
{
    public class PagarPublicacionCommandValidator : AbstractValidator<PagarPublicacionCommand>
    {
        public PagarPublicacionCommandValidator()
        {
            RuleFor(command => command.EventoId)
                .NotEmpty()
                .WithMessage("El identificador del evento es requerido.");

            RuleFor(command => command.TransaccionPagoId)
                .NotEmpty()
                .WithMessage("La transacción de pago es requerida.");

            RuleFor(command => command.Monto)
                .GreaterThan(0)
                .WithMessage("El monto abonado debe ser mayor a cero.");
        }
    }
}
