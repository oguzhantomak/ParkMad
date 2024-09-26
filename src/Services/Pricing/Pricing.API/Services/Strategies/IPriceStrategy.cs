namespace Pricing.API.Services.Strategies;

public interface IPriceStrategy
{
    decimal CalculatePrice(TimeSpan duration);
}