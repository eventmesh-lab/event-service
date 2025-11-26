using System;
using MediatR;

namespace event_service.Application.Commands.PublicarEvento
{
    public record PublicarEventoCommand : IRequest
    {
        public Guid EventoId { get; init; }
        public Guid PagoConfirmadoId { get; init; }
    }
}
