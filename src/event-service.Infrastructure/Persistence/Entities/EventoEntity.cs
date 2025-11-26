using System;
using System.Collections.Generic;

namespace event_service.Infrastructure.Persistence.Entities
{
    public class EventoEntity
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; } = null!;
        public string? Descripcion { get; set; }
        public Guid OrganizadorId { get; set; }
        public Guid VenueId { get; set; }
        public string Categoria { get; set; } = null!;
        public decimal TarifaPublicacion { get; set; }
        public DateTime FechaInicio { get; set; }
        public int DuracionHoras { get; set; }
        public int DuracionMinutos { get; set; }
        public string Estado { get; set; } = null!;
        public Guid? TransaccionPagoId { get; set; }
        public int Version { get; set; }
        public List<SeccionEntity> Secciones { get; set; } = new();
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaPublicacion { get; set; }
    }
}
