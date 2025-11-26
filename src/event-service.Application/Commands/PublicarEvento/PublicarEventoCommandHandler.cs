using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using event_service.Domain.Events;
using event_service.Domain.Ports;

namespace event_service.Application.Commands.PublicarEvento
{
    public class PublicarEventoCommandHandler : IRequestHandler<PublicarEventoCommand>
    {
        private readonly IEventoRepository _repository;
        private readonly IDomainEventPublisher _domainEventPublisher;
        private readonly IValidator<PublicarEventoCommand> _validator;

        public PublicarEventoCommandHandler(
            IEventoRepository repository,
            IDomainEventPublisher domainEventPublisher,
            IValidator<PublicarEventoCommand> validator)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _domainEventPublisher = domainEventPublisher ?? throw new ArgumentNullException(nameof(domainEventPublisher));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        public async Task Handle(PublicarEventoCommand request, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var evento = await _repository.GetByIdAsync(request.EventoId, cancellationToken);
            if (evento == null)
            {
                throw new InvalidOperationException($"El evento con ID {request.EventoId} no existe.");
            }

            evento.Publicar(request.PagoConfirmadoId, DateTime.Now);

            await _repository.UpdateAsync(evento, cancellationToken);

            var domainEvents = evento.GetDomainEvents();
            foreach (var domainEvent in domainEvents)
            {
                await _domainEventPublisher.PublishAsync(domainEvent, cancellationToken);
            }
            evento.ClearDomainEvents();
        }
    }
}
