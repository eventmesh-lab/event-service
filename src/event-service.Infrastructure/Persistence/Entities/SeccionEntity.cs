using System;

namespace event_service.Infrastructure.Persistence.Entities
{
    public class SeccionEntity
    {
        public Guid Id { get; set; }
        public Guid EventoId { get; set; }
        public string Nombre { get; set; } = null!;
        public decimal PrecioMonto { get; set; }
        public int Capacidad { get; set; }
        public EventoEntity? Evento { get; set; }
    }
}
