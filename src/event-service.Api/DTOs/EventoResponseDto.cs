using System;
using System.Collections.Generic;
using System.Linq;
using event_service.Domain.Entities;
using event_service.Domain.ValueObjects;

namespace event_service.Api.DTOs
{
    public record EventoResponseDto
    {
        public Guid Id { get; init; }
        public string Nombre { get; init; } = string.Empty;
        public string Descripcion { get; init; } = string.Empty;
        public DateTime Fecha { get; init; }
        public int HorasDuracion { get; init; }
        public int MinutosDuracion { get; init; }
        public string Estado { get; init; } = string.Empty;
        public Guid OrganizadorId { get; init; }
        public Guid VenueId { get; init; }
        public string Categoria { get; init; } = string.Empty;
        public decimal TarifaPublicacion { get; init; }
        public Guid? TransaccionPagoId { get; init; }
        public DateTime FechaCreacion { get; init; }
        public DateTime? FechaPublicacion { get; init; }
        public int Version { get; init; }
        public List<SeccionResponseDto> Secciones { get; init; } = new();

        public static EventoResponseDto FromDomain(Evento evento)
        {
            if (evento == null) throw new ArgumentNullException(nameof(evento));

            return new EventoResponseDto
            {
                Id = evento.Id,
                Nombre = evento.Nombre,
                Descripcion = evento.Descripcion,
                Fecha = evento.Fecha.Valor,
                HorasDuracion = evento.Duracion.Horas,
                MinutosDuracion = evento.Duracion.Minutos,
                Estado = evento.Estado.Valor,
                OrganizadorId = evento.OrganizadorId,
                VenueId = evento.VenueId,
                Categoria = evento.Categoria,
                TarifaPublicacion = evento.TarifaPublicacion,
                TransaccionPagoId = evento.TransaccionPagoId,
                FechaCreacion = evento.FechaCreacion,
                FechaPublicacion = evento.FechaPublicacion,
                Version = evento.Version,
                Secciones = evento.Secciones.Select(s => SeccionResponseDto.FromDomain(s)).ToList()
            };
        }
    }

    public record SeccionResponseDto
    {
        public Guid Id { get; init; }
        public string Nombre { get; init; } = string.Empty;
        public int Capacidad { get; init; }
        public decimal Precio { get; init; }

        public static SeccionResponseDto FromDomain(Seccion seccion)
        {
            if (seccion == null) throw new ArgumentNullException(nameof(seccion));

            return new SeccionResponseDto
            {
                Id = seccion.Id,
                Nombre = seccion.Nombre,
                Capacidad = seccion.Capacidad,
                Precio = seccion.Precio.Valor
            };
        }
    }
}
