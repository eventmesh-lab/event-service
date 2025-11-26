using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using event_service.Domain.Entities;
using event_service.Domain.Events;
using event_service.Domain.Ports;
using event_service.Domain.ValueObjects;

namespace event_service.Application.Commands.CrearEvento
{
    public class CrearEventoCommandHandler : IRequestHandler<CrearEventoCommand, CrearEventoCommandResponse>
    {
        private readonly IEventoRepository _repository;
        private readonly IDomainEventPublisher _domainEventPublisher;
        private readonly IValidator<CrearEventoCommand> _validator;

        public CrearEventoCommandHandler(
            IEventoRepository repository,
            IDomainEventPublisher domainEventPublisher,
            IValidator<CrearEventoCommand> validator)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _domainEventPublisher = domainEventPublisher ?? throw new ArgumentNullException(nameof(domainEventPublisher));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        public async Task<CrearEventoCommandResponse> Handle(CrearEventoCommand request, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var duracion = new DuracionEvento(request.HorasDuracion, request.MinutosDuracion);
            var fechaEvento = new FechaEvento(request.Fecha);
            var secciones = request.Secciones.Select(s => new Seccion(s.Nombre, s.Capacidad, new PrecioEntrada(s.Precio))).ToList();

            var evento = Evento.Crear(
                request.Nombre,
                request.Descripcion,
                fechaEvento,
                duracion,
                request.OrganizadorId,
                request.VenueId,
                request.Categoria,
                request.TarifaPublicacion,
                secciones
            );

            await _repository.AddAsync(evento, cancellationToken);

            var domainEvents = evento.GetDomainEvents();
            foreach (var domainEvent in domainEvents)
            {
                await _domainEventPublisher.PublishAsync(domainEvent, cancellationToken);
            }
            evento.ClearDomainEvents();

            return new CrearEventoCommandResponse { Id = evento.Id };
        }
    }
}
