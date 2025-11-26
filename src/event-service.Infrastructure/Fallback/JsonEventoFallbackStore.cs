using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using event_service.Domain.Entities;

namespace event_service.Infrastructure.Fallback
{
    public class JsonEventoFallbackStore : IEventoFallbackStore
    {
        private readonly EventoFallbackOptions _options;

        public JsonEventoFallbackStore(EventoFallbackOptions options)
        {
            _options = options;
        }

        private string Resolve() => _options.ResolvePath();

        public async Task SaveAsync(Evento evento)
        {
            var path = Resolve();
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);

            var text = JsonConvert.SerializeObject(evento);
            await File.WriteAllTextAsync(path, text);
        }

        public async Task<Evento?> LoadAsync(Guid id)
        {
            var path = Resolve();
            if (!File.Exists(path)) return null;

            var text = await File.ReadAllTextAsync(path);
            try
            {
                var dto = JsonConvert.DeserializeObject<Evento>(text);
                return dto;
            }
            catch
            {
                return null;
            }
        }
    }
}
