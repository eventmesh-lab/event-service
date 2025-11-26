using System;
using System.Collections.Generic;
using MediatR;

namespace event_service.Application.Commands.CrearEvento
{
    /// <summary>
    /// Comando para crear un nuevo evento en estado borrador.
    /// </summary>
    public record CrearEventoCommand : IRequest<CrearEventoCommandResponse>
    {
        public string Nombre { get; init; } = string.Empty;
        public string Descripcion { get; init; } = string.Empty;
        public DateTime Fecha { get; init; }
        public int HorasDuracion { get; init; }
        public int MinutosDuracion { get; init; }
        public List<SeccionDto> Secciones { get; init; } = new();
        public Guid OrganizadorId { get; init; }
        public Guid VenueId { get; init; }
        public string Categoria { get; init; } = string.Empty;
        public decimal TarifaPublicacion { get; init; }

        public record SeccionDto
        {
            public string Nombre { get; init; } = string.Empty;
            public int Capacidad { get; init; }
            public decimal Precio { get; init; }
            public string? TipoAsiento { get; init; }
        }
    }

    public record CrearEventoCommandResponse
    {
        public Guid Id { get; init; }
    }
}
