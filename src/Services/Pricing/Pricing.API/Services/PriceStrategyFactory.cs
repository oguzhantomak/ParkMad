using Pricing.API.Services.Strategies;

namespace Pricing.API.Services;

public class PriceStrategyFactory : IPriceStrategyFactory
{
    public IPriceStrategy GetStrategy(string regionName)
    {
        return regionName switch
        {
            "A" => new RegionAPriceStrategy(),
            "B" => new RegionBPriceStrategy(),
            "C" => new RegionCPriceStrategy(),
            _ => throw new Exception("Geçersiz bölge adı.")
        };
    }
}