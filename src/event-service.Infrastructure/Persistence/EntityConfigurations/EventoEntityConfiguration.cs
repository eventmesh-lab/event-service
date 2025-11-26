using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using event_service.Infrastructure.Persistence.Entities;

namespace event_service.Infrastructure.Persistence.EntityConfigurations
{
    public class EventoEntityConfiguration : IEntityTypeConfiguration<EventoEntity>
    {
        public void Configure(EntityTypeBuilder<EventoEntity> builder)
        {
            builder.ToTable("eventos");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id).HasColumnName("id").HasColumnType("uuid");
            builder.Property(x => x.Nombre).HasColumnName("nombre").HasMaxLength(250).IsRequired();
            builder.Property(x => x.Descripcion).HasColumnName("descripcion");
            builder.Property(x => x.OrganizadorId).HasColumnName("organizador_id").HasColumnType("uuid");
            builder.Property(x => x.VenueId).HasColumnName("venue_id").HasColumnType("uuid");
            builder.Property(x => x.Categoria).HasColumnName("categoria").HasMaxLength(100);
            builder.Property(x => x.TarifaPublicacion).HasColumnName("tarifa_publicacion").HasColumnType("numeric(10,2)");
            builder.Property(x => x.FechaInicio).HasColumnName("fecha_inicio");
            builder.Property(x => x.DuracionHoras).HasColumnName("duracion_horas");
            builder.Property(x => x.DuracionMinutos).HasColumnName("duracion_minutos");
            builder.Property(x => x.Estado).HasColumnName("estado").HasMaxLength(50);
            builder.Property(x => x.TransaccionPagoId).HasColumnName("transaccion_pago_id").HasColumnType("uuid");
            builder.Property(x => x.Version).HasColumnName("version");
            builder.Property(x => x.FechaCreacion).HasColumnName("fecha_creacion");
            builder.Property(x => x.FechaPublicacion).HasColumnName("fecha_publicacion");

            builder.HasMany(e => e.Secciones)
                   .WithOne(s => s.Evento)
                   .HasForeignKey(s => s.EventoId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
