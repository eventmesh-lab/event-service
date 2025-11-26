using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using event_service.Domain.Entities;
using event_service.Domain.Ports;
using event_service.Infrastructure.Persistence.Entities;

namespace event_service.Infrastructure.Repositories
{
    public class EventoRepository : IEventoRepository
    {
        private readonly event_service.Infrastructure.Persistence.EventsDbContext _db;

        public EventoRepository(event_service.Infrastructure.Persistence.EventsDbContext db)
        {
            _db = db;
        }

        public async Task AddAsync(Evento evento, System.Threading.CancellationToken cancellationToken = default)
        {
            var existing = await _db.Eventos.Include(e => e.Secciones).FirstOrDefaultAsync(e => e.Id == evento.Id, cancellationToken);

            if (existing is null)
            {
                var entity = ToEntity(evento);
                _db.Eventos.Add(entity);
            }
            else
            {
                var entity = ToEntity(evento);
                _db.Entry(existing).CurrentValues.SetValues(entity);
                _db.Secciones.RemoveRange(existing.Secciones);
                existing.Secciones = entity.Secciones;
            }

            await _db.SaveChangesAsync(cancellationToken);
        }

        public async Task<Evento?> GetByIdAsync(Guid id, System.Threading.CancellationToken cancellationToken = default)
        {
            var e = await _db.Eventos.Include(x => x.Secciones).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            if (e is null) return null;
            return ToDomain(e);
        }

        public async Task UpdateAsync(Evento evento, System.Threading.CancellationToken cancellationToken = default)
        {
            await AddAsync(evento, cancellationToken);
        }


        private static Evento ToDomain(EventoEntity e)
        {
            // Create domain Seccion instances
            var secciones = e.Secciones.Select(s => new event_service.Domain.Entities.Seccion(s.Id, s.Nombre, s.Capacidad, new event_service.Domain.ValueObjects.PrecioEntrada(s.PrecioMonto))).ToList();

            var fecha = new event_service.Domain.ValueObjects.FechaEvento(e.FechaInicio);
            var duracion = new event_service.Domain.ValueObjects.DuracionEvento(e.DuracionHoras, e.DuracionMinutos);

            // Rehydrate Evento using uninitialized object and reflection to set private setters
            var tipoEvento = typeof(Evento);
            var instancia = System.Runtime.Serialization.FormatterServices.GetUninitializedObject(tipoEvento) as Evento;

            // Set properties via reflection
            void Set(string name, object? value)
            {
                var prop = tipoEvento.GetProperty(name, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                if (prop != null)
                {
                    prop.SetValue(instancia, value);
                }
            }

            Set("Id", e.Id);
            Set("Nombre", e.Nombre);
            Set("Descripcion", e.Descripcion ?? string.Empty);
            Set("Fecha", fecha);
            Set("Duracion", duracion);
            Set("Estado", new event_service.Domain.ValueObjects.EstadoEvento(e.Estado));
            Set("OrganizadorId", e.OrganizadorId);
            Set("VenueId", e.VenueId);
            Set("Categoria", e.Categoria);
            Set("TarifaPublicacion", e.TarifaPublicacion);
            Set("TransaccionPagoId", e.TransaccionPagoId);
            Set("FechaCreacion", e.FechaCreacion);
            Set("FechaPublicacion", e.FechaPublicacion);
            Set("Version", e.Version);

            // Fill secciones by accessing the private list field
            var field = tipoEvento.GetField("_secciones", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (field != null)
            {
                var domainSecciones = secciones;
                field.SetValue(instancia, domainSecciones);
            }

            return instancia!;
        }

        private static EventoEntity ToEntity(Evento evento)
        {
            var entity = new EventoEntity
            {
                Id = evento.Id,
                Nombre = evento.Nombre,
                Descripcion = evento.Descripcion,
                OrganizadorId = evento.OrganizadorId,
                VenueId = evento.VenueId,
                Categoria = evento.Categoria,
                TarifaPublicacion = evento.TarifaPublicacion,
                FechaInicio = evento.Fecha.Valor,
                DuracionHoras = evento.Duracion.Horas,
                DuracionMinutos = evento.Duracion.Minutos,
                Estado = evento.Estado.Valor,
                TransaccionPagoId = evento.TransaccionPagoId,
                Version = evento.Version,
                FechaCreacion = evento.FechaCreacion,
                FechaPublicacion = evento.FechaPublicacion
            };

            entity.Secciones = evento.Secciones.Select(s => new SeccionEntity
            {
                Id = s.Id,
                EventoId = evento.Id,
                Nombre = s.Nombre,
                Capacidad = s.Capacidad,
                PrecioMonto = s.Precio.Valor
            }).ToList();

            return entity;
        }
    }
}
