using FluentValidation;

namespace event_service.Application.Commands.FinalizarEvento
{
    public class FinalizarEventoCommandValidator : AbstractValidator<FinalizarEventoCommand>
    {
        public FinalizarEventoCommandValidator()
        {
            RuleFor(command => command.EventoId)
                .NotEmpty()
                .WithMessage("El identificador del evento es requerido.");
        }
    }
}
