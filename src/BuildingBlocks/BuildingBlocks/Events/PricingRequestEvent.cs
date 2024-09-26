namespace BuildingBlocks.Events;

public class PricingRequestEvent
{
    public string PlateNumber { get; set; }
    public double Duration { get; set; }
    public string ZoneName { get; set; }
}