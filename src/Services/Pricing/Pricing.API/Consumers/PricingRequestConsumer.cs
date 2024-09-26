namespace Pricing.API.Consumers;
public class PricingRequestConsumer : IConsumer<PricingRequestEvent>
{
    private readonly ILogger<PricingRequestConsumer> _logger;
    private readonly IPriceStrategyFactory _strategyFactory;
    private readonly IDistributedCache _distributedCache;

    public PricingRequestConsumer(
        ILogger<PricingRequestConsumer> logger,
        IPriceStrategyFactory strategyFactory,
        IDistributedCache distributedCache)
    {
        _logger = logger;
        _strategyFactory = strategyFactory;
        _distributedCache = distributedCache;
    }

    public async Task Consume(ConsumeContext<PricingRequestEvent> context)
    {
        try
        {
            var pricingRequest = context.Message;

            _logger.LogInformation("Calculating price for region: {RegionName}", pricingRequest.ZoneName);

            var strategy = _strategyFactory.GetStrategy(pricingRequest.ZoneName);

            var duration = TimeSpan.FromHours(pricingRequest.Duration);
            var price = strategy.CalculatePrice(duration);

            _logger.LogInformation("Calculated price: {Price}", price);

            var cacheKey = $"PricingResponse_{pricingRequest.PlateNumber}";
            var priceData = new PriceResponseDto { Price = price };

            await _distributedCache.SetStringAsync(cacheKey, JsonSerializer.Serialize(priceData),
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
                });

            _logger.LogInformation("Price cached in Redis: {CacheKey}", cacheKey);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}