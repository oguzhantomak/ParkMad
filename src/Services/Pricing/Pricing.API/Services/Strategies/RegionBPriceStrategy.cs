namespace Pricing.API.Services.Strategies;

public class RegionBPriceStrategy : IPriceStrategy
{
    public decimal CalculatePrice(TimeSpan duration)
    {
        return (decimal)duration.TotalHours * 40;
    }
}