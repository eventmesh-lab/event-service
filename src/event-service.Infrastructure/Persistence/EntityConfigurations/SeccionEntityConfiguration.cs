using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using event_service.Infrastructure.Persistence.Entities;

namespace event_service.Infrastructure.Persistence.EntityConfigurations
{
    public class SeccionEntityConfiguration : IEntityTypeConfiguration<SeccionEntity>
    {
        public void Configure(EntityTypeBuilder<SeccionEntity> builder)
        {
            builder.ToTable("secciones");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id).HasColumnName("id").HasColumnType("uuid");
            builder.Property(x => x.EventoId).HasColumnName("evento_id").HasColumnType("uuid");
            builder.Property(x => x.Nombre).HasColumnName("nombre").HasMaxLength(200).IsRequired();
            builder.Property(x => x.PrecioMonto).HasColumnName("precio_monto").HasColumnType("numeric(10,2)");
            builder.Property(x => x.Capacidad).HasColumnName("capacidad");

            builder.HasIndex(x => x.EventoId).HasDatabaseName("IX_secciones_evento_id");
        }
    }
}
