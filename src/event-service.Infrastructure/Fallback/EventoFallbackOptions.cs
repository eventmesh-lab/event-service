using System;
using System.IO;

namespace event_service.Infrastructure.Fallback
{
    public class EventoFallbackOptions
    {
        public string? EventsFilePath { get; set; }

        public string ResolvePath()
        {
            if (!string.IsNullOrWhiteSpace(EventsFilePath)) return EventsFilePath!;

            var baseDir = AppContext.BaseDirectory ?? Directory.GetCurrentDirectory();
            var path = Path.Combine(baseDir, "App_Data", "events-fallback.json");
            return path;
        }
    }
}
