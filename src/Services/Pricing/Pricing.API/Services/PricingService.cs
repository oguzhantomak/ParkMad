namespace Pricing.API.Services;

public class PricingService(IPriceStrategyFactory strategyFactory, ILogger<PricingService> logger)
    : IPricingService
{
    public decimal CalculatePrice(string regionName, TimeSpan duration)
    {
        logger.LogInformation("Bölge için fiyat hesaplanıyor: {RegionName}", regionName);

        var strategy = strategyFactory.GetStrategy(regionName);
        var price = strategy.CalculatePrice(duration);

        logger.LogInformation("Hesaplanan fiyat: {Price}", price);

        return price;
    }
}