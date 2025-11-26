using System;
using MediatR;

namespace event_service.Application.Commands.PagarPublicacion
{
    public record PagarPublicacionCommand : IRequest
    {
        public Guid EventoId { get; init; }
        public Guid TransaccionPagoId { get; init; }
        public decimal Monto { get; init; }
    }
}
