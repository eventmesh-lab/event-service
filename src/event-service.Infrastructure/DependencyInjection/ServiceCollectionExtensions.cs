using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using event_service.Domain.Ports;
using event_service.Infrastructure.Persistence;
using event_service.Infrastructure.Repositories;
using event_service.Infrastructure.Fallback;
using event_service.Infrastructure.Messaging;

namespace event_service.Infrastructure.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // DbContext registration: if a connection string exists, use Npgsql, otherwise use InMemory for tests
            var conn = configuration.GetConnectionString("Events");
            if (!string.IsNullOrWhiteSpace(conn))
            {
                services.AddDbContext<EventsDbContext>(opts => opts.UseNpgsql(conn));
            }
            else
            {
                services.AddDbContext<EventsDbContext>(opts => opts.UseInMemoryDatabase("events-in-memory"));
            }

            // Fallback options and store
            var fallbackOptions = new EventoFallbackOptions();
            configuration.Bind("EventoFallback", fallbackOptions);
            services.AddSingleton(fallbackOptions);
            services.AddSingleton<IEventoFallbackStore, JsonEventoFallbackStore>();

            // Repositories: primary + hybrid
            services.AddScoped<EventoRepository>();
            services.AddScoped<IEventoRepository>(sp =>
            {
                var primary = sp.GetRequiredService<EventoRepository>();
                var fallback = sp.GetRequiredService<IEventoFallbackStore>();
                return new HybridEventoRepository(primary, fallback);
            });

            // Messaging
            services.AddRabbitMqMessaging(configuration);

            return services;
        }
    }
}
