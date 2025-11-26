using Microsoft.EntityFrameworkCore;
using event_service.Infrastructure.Persistence.Entities;
using event_service.Infrastructure.Persistence.EntityConfigurations;

namespace event_service.Infrastructure.Persistence
{
    public class EventsDbContext : DbContext
    {
        public EventsDbContext(DbContextOptions<EventsDbContext> options) : base(options)
        {
        }

        public DbSet<EventoEntity> Eventos { get; set; } = null!;
        public DbSet<SeccionEntity> Secciones { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new EventoEntityConfiguration());
            modelBuilder.ApplyConfiguration(new SeccionEntityConfiguration());
            base.OnModelCreating(modelBuilder);
        }
    }
}
