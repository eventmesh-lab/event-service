using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using event_service.Domain.Entities;
using event_service.Domain.Ports;
using event_service.Domain.ValueObjects;

namespace event_service.Application.Commands.EditarEvento
{
    public class EditarEventoCommandHandler : IRequestHandler<EditarEventoCommand>
    {
        private readonly IEventoRepository _repository;
        private readonly IDomainEventPublisher _domainEventPublisher;
        private readonly IValidator<EditarEventoCommand> _validator;

        public EditarEventoCommandHandler(
            IEventoRepository repository,
            IDomainEventPublisher domainEventPublisher,
            IValidator<EditarEventoCommand> validator)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _domainEventPublisher = domainEventPublisher ?? throw new ArgumentNullException(nameof(domainEventPublisher));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        public async Task Handle(EditarEventoCommand request, CancellationToken cancellationToken)
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

            var fechaEvento = new FechaEvento(request.Fecha);
            var duracionEvento = new DuracionEvento(request.HorasDuracion, request.MinutosDuracion);
            var secciones = MapearSecciones(request.Secciones);

            evento.Editar(
                request.Nombre,
                request.Descripcion,
                fechaEvento,
                duracionEvento,
                request.Categoria,
                secciones);

            await _repository.UpdateAsync(evento, cancellationToken);

            await PublicarEventosDeDominio(evento, cancellationToken);
        }

        private static IReadOnlyCollection<Seccion> MapearSecciones(IEnumerable<EditarEventoCommand.SeccionDto> seccionesDto)
        {
            return seccionesDto
                .Select(dto =>
                {
                    var precio = new PrecioEntrada(dto.Precio);
                    return dto.Id.HasValue
                        ? new Seccion(dto.Id.Value, dto.Nombre, dto.Capacidad, precio)
                        : new Seccion(dto.Nombre, dto.Capacidad, precio);
                })
                .ToList();
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
