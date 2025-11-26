using System;
using MediatR;

namespace event_service.Application.Commands.FinalizarEvento
{
    public record FinalizarEventoCommand : IRequest
    {
        public Guid EventoId { get; init; }
    }
}
