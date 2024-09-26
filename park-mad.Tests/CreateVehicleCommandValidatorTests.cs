using BuildingBlocks.Common.Enums;
using FluentAssertions;
using Vehicle.API.Vehicles.CreateVehicle;

namespace park_mad.Tests;

public class CreateVehicleCommandValidatorTests
{
    private readonly CreateVehicleCommandValidator _validator;

    public CreateVehicleCommandValidatorTests()
    {
        _validator = new CreateVehicleCommandValidator();
    }

    [Theory]
    [InlineData("01ABC123", VehicleSize.Small, true)]
    [InlineData("34ABC789", VehicleSize.Medium, true)]
    [InlineData("", VehicleSize.Large, false)]
    [InlineData(null, VehicleSize.Large, false)]
    [InlineData("INVALID", VehicleSize.Small, false)]
    [InlineData("99AAA999", (VehicleSize)999, false)]
    public void Validate_ShouldValidatePlateNumberAndVehicleSize(string plateNumber, VehicleSize vehicleSize, bool expectedIsValid)
    {
        var command = new CreateVehicleCommand(plateNumber, vehicleSize);

        var result = _validator.Validate(command);

        result.IsValid.Should().Be(expectedIsValid);
    }
}