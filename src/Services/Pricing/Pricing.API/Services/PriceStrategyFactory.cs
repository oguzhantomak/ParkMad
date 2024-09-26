using Pricing.API.Services.Strategies;

namespace Pricing.API.Services;

public class PriceStrategyFactory : IPriceStrategyFactory
{
    public IPriceStrategy GetStrategy(string regionName)
    {
        return regionName switch
        {
            "Zone A" => new RegionAPriceStrategy(),
            "Zone B" => new RegionBPriceStrategy(),
            "Zone C" => new RegionCPriceStrategy(),
            _ => throw new Exception("Geçersiz bölge adı.")
        };
    }
}