using System;
using Xunit;
using event_service.Domain.Entities;

namespace event_service.Domain.Tests
{
    public class ExampleAggregateTests
    {
        [Fact]
        public void Ctor_WithName_SetsProperties()
        {
            var name = "test";
            var agg = new ExampleAggregate(name);

            Assert.Equal(name, agg.Name);
            Assert.NotEqual(Guid.Empty, agg.Id);
            Assert.True(agg.CreatedAt <= DateTime.UtcNow);
        }
    }
}
