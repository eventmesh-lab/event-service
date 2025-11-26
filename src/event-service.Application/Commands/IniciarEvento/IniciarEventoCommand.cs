using System;
using MediatR;

namespace event_service.Application.Commands.IniciarEvento
{
    public record IniciarEventoCommand : IRequest
    {
        public Guid EventoId { get; init; }
    }
}
