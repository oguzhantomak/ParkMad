namespace Pricing.API.Services;

public interface IPricingService
{
    decimal CalculatePrice(string regionName, TimeSpan duration);

}