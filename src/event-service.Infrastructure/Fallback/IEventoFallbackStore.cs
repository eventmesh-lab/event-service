using System;
using System.Threading.Tasks;
using event_service.Domain.Entities;

namespace event_service.Infrastructure.Fallback
{
    public interface IEventoFallbackStore
    {
        Task SaveAsync(event_service.Domain.Entities.Evento evento);
        Task<event_service.Domain.Entities.Evento?> LoadAsync(Guid id);
    }
}
