using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using event_service.Domain.Entities;
using event_service.Domain.Ports;

namespace event_service.Application.Commands.PagarPublicacion
{
    public class PagarPublicacionCommandHandler : IRequestHandler<PagarPublicacionCommand>
    {
        private readonly IEventoRepository _repository;
        private readonly IDomainEventPublisher _domainEventPublisher;
        private readonly IValidator<PagarPublicacionCommand> _validator;

        public PagarPublicacionCommandHandler(
            IEventoRepository repository,
            IDomainEventPublisher domainEventPublisher,
            IValidator<PagarPublicacionCommand> validator)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _domainEventPublisher = domainEventPublisher ?? throw new ArgumentNullException(nameof(domainEventPublisher));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        public async Task Handle(PagarPublicacionCommand request, CancellationToken cancellationToken)
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

            evento.PagarPublicacion(request.TransaccionPagoId, request.Monto);

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
