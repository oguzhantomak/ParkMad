using Pricing.API.Services.Strategies;

namespace Pricing.API.Services;

public interface IPriceStrategyFactory
{
    IPriceStrategy GetStrategy(string regionName);
}