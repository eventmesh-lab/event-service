using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using event_service.Domain.Entities;
using event_service.Domain.Ports;

namespace event_service.Application.Commands.IniciarEvento
{
    public class IniciarEventoCommandHandler : IRequestHandler<IniciarEventoCommand>
    {
        private readonly IEventoRepository _repository;
        private readonly IDomainEventPublisher _domainEventPublisher;
        private readonly IValidator<IniciarEventoCommand> _validator;

        public IniciarEventoCommandHandler(
            IEventoRepository repository,
            IDomainEventPublisher domainEventPublisher,
            IValidator<IniciarEventoCommand> validator)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _domainEventPublisher = domainEventPublisher ?? throw new ArgumentNullException(nameof(domainEventPublisher));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        public async Task Handle(IniciarEventoCommand request, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var evento = await _repository.GetByIdAsync(request.EventoId, cancellationToken);
            if (evento is null)
            {
                throw new InvalidOperationException($"El evento con ID {request.EventoId} no existe.");
            }

            evento.Iniciar(DateTime.UtcNow);

            await _repository.UpdateAsync(evento, cancellationToken);
            await PublicarEventosDeDominio(evento, cancellationToken);
        }

        private async Task PublicarEventosDeDominio(Evento evento, CancellationToken cancellationToken)
        {
            foreach (var domainEvent in evento.GetDomainEvents())
            {
                await _domainEventPublisher.PublishAsync(domainEvent, cancellationToken);
            }

            evento.ClearDomainEvents();
        }
    }
}
