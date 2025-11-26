using System;
using System.Threading;
using System.Threading.Tasks;
using event_service.Domain.Entities;

namespace event_service.Domain.Ports
{
    public interface IExampleRepository
    {
        Task AddAsync(ExampleAggregate entity, CancellationToken cancellationToken = default);
        Task<ExampleAggregate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
