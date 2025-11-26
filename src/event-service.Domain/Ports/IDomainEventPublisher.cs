using System.Threading;
using System.Threading.Tasks;
using event_service.Domain.Events;

namespace event_service.Domain.Ports
{
    /// <summary>
    /// Interfaz para publicar eventos de dominio en el bus de mensajería.
    /// Define el contrato para la publicación de eventos de dominio.
    /// </summary>
    public interface IDomainEventPublisher
    {
        /// <summary>
        /// Publica un evento de dominio en el bus de mensajería.
        /// </summary>
        /// <param name="domainEvent">Evento de dominio a publicar.</param>
        /// <param name="cancellationToken">Token de cancelación.</param>
        Task PublishAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default);
    }
}
