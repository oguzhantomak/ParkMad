using BuildingBlocks.Events;
using MassTransit;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Moq;
using Pricing.API.Consumers;
using Pricing.API.Services.Strategies;
using Pricing.API.Services;

namespace park_mad.Tests;

public class PricingRequestConsumerTests
{
    private readonly Mock<ILogger<PricingRequestConsumer>> _loggerMock;
    private readonly Mock<IPriceStrategyFactory> _strategyFactoryMock;
    private readonly Mock<IDistributedCache> _cacheMock;
    private readonly PricingRequestConsumer _consumer;

    public PricingRequestConsumerTests()
    {
        _loggerMock = new Mock<ILogger<PricingRequestConsumer>>();
        _strategyFactoryMock = new Mock<IPriceStrategyFactory>();
        _cacheMock = new Mock<IDistributedCache>();

        _consumer = new PricingRequestConsumer(
            _loggerMock.Object,
            _strategyFactoryMock.Object,
            _cacheMock.Object);
    }

    [Fact]
    public async Task Consume_ShouldCalculatePrice_AndCacheResult()
    {
        // Arrange
        var pricingRequestEvent = new PricingRequestEvent
        {
            PlateNumber = "34ABC78",
            Duration = 2,
            ZoneName = "A"
        };

        var contextMock = new Mock<ConsumeContext<PricingRequestEvent>>();
        contextMock.Setup(c => c.Message).Returns(pricingRequestEvent);

        var strategyMock = new Mock<IPriceStrategy>();
        strategyMock.Setup(s => s.CalculatePrice(It.IsAny<TimeSpan>())).Returns(20);

        _strategyFactoryMock.Setup(f => f.GetStrategy(pricingRequestEvent.ZoneName))
            .Returns(strategyMock.Object);

        // Act
        await _consumer.Consume(contextMock.Object);

        // Assert
        _cacheMock.Verify(c => c.SetStringAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<DistributedCacheEntryOptions>(),
            default), Times.Once);
    }
}