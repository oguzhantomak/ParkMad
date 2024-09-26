using MassTransit;
using Microsoft.Extensions.Caching.Distributed;
using Pricing.API.Events;
using Pricing.API.Services;
using System.Text.Json;
using Pricing.API.Models.DTOs;

namespace Pricing.API.Consumers;

public class PricingRequestConsumer(
    ILogger<PricingRequestConsumer> logger,
    IPriceStrategyFactory strategyFactory,
    IDistributedCache distributedCache)
    : IConsumer<PricingRequestEvent>
{
    public async Task Consume(ConsumeContext<PricingRequestEvent> context)
    {
        var pricingRequest = context.Message;

        logger.LogInformation("Bölge için fiyat hesaplanıyor: {RegionName}", pricingRequest.ZoneName);

        var strategy = strategyFactory.GetStrategy(pricingRequest.ZoneName);

        var duration = TimeSpan.FromHours(pricingRequest.Duration);
        var price = strategy.CalculatePrice(duration);

        logger.LogInformation("Hesaplanan fiyat: {Price}", price);


        var cacheKey = $"PricingResponse_{pricingRequest.PlateNumber}";
        var priceData = new PriceResponseDto { Price = price };

        await distributedCache.SetStringAsync(cacheKey, JsonSerializer.Serialize(priceData), new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
        });

        logger.LogInformation("Fiyat Redis'e cache'lendi: {CacheKey}", cacheKey);
    }
}