using System;
using System.Collections.Generic;
using MediatR;

namespace event_service.Application.Commands.EditarEvento
{
    public record EditarEventoCommand : IRequest
    {
        public Guid EventoId { get; init; }
        public string Nombre { get; init; } = string.Empty;
        public string Descripcion { get; init; } = string.Empty;
        public DateTime Fecha { get; init; }
        public int HorasDuracion { get; init; }
        public int MinutosDuracion { get; init; }
        public string Categoria { get; init; } = string.Empty;
        public List<SeccionDto> Secciones { get; init; } = new();

        public record SeccionDto
        {
            public Guid? Id { get; init; }
            public string Nombre { get; init; } = string.Empty;
            public int Capacidad { get; init; }
            public decimal Precio { get; init; }
        }
    }
}
