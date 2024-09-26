namespace Pricing.API.Services.Strategies;

public class RegionAPriceStrategy : IPriceStrategy
{
    public decimal CalculatePrice(TimeSpan duration)
    {
        return (decimal)duration.TotalHours * 50;
    }
}