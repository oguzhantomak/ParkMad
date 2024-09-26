namespace Pricing.API.Services.Strategies;

public class RegionCPriceStrategy : IPriceStrategy
{
    public decimal CalculatePrice(TimeSpan duration)
    {
        return (decimal)duration.TotalHours * 30;
    }
}