using FluentValidation;

namespace event_service.Application.Commands.PublicarEvento
{
    public class PublicarEventoCommandValidator : AbstractValidator<PublicarEventoCommand>
    {
        public PublicarEventoCommandValidator()
        {
            RuleFor(command => command.EventoId)
                .NotEmpty()
                .WithMessage("El identificador del evento es requerido.");

            RuleFor(command => command.PagoConfirmadoId)
                .NotEmpty()
                .WithMessage("El identificador del pago confirmado es requerido.");
        }
    }
}

