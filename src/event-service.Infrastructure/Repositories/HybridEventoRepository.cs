using System;
using System.Threading.Tasks;
using event_service.Domain.Entities;
using event_service.Domain.Ports;
using event_service.Infrastructure.Fallback;

namespace event_service.Infrastructure.Repositories
{
    public class HybridEventoRepository : IEventoRepository
    {
        private readonly IEventoRepository _primary;
        private readonly IEventoFallbackStore _fallback;

        public HybridEventoRepository(IEventoRepository primary, IEventoFallbackStore fallback)
        {
            _primary = primary;
            _fallback = fallback;
        }

        public async Task AddAsync(Evento evento, System.Threading.CancellationToken cancellationToken = default)
        {
            try
            {
                await _primary.AddAsync(evento, cancellationToken);
            }
            catch
            {
                await _fallback.SaveAsync(evento);
            }
        }

        public async Task<Evento?> GetByIdAsync(Guid id, System.Threading.CancellationToken cancellationToken = default)
        {
            try
            {
                var fromPrimary = await _primary.GetByIdAsync(id, cancellationToken);
                if (fromPrimary != null) return fromPrimary;
            }
            catch
            {
                // ignore and try fallback
            }

            return await _fallback.LoadAsync(id);
        }

        public async Task UpdateAsync(Evento evento, System.Threading.CancellationToken cancellationToken = default)
        {
            await AddAsync(evento, cancellationToken);
        }
    }
}
