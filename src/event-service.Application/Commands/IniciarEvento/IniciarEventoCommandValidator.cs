using FluentValidation;

namespace event_service.Application.Commands.IniciarEvento
{
    public class IniciarEventoCommandValidator : AbstractValidator<IniciarEventoCommand>
    {
        public IniciarEventoCommandValidator()
        {
            RuleFor(command => command.EventoId)
                .NotEmpty()
                .WithMessage("El identificador del evento es requerido.");
        }
    }
}
