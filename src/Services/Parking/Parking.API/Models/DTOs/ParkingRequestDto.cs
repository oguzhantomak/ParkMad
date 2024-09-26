namespace Parking.API.Models.DTOs;

public class ParkingRequestDto : IValidatable
{
    public string PlateNumber { get; set; }

    public async Task<FluentValidation.Results.ValidationResult> ValidateAsync()
    {
        var validationResult = new FluentValidation.Results.ValidationResult();

        if (string.IsNullOrEmpty(PlateNumber))
        {
            validationResult.Errors.Add(new ValidationFailure("PlateNumber", "Plate number is required."));
        }
        else
        {
            var plateRegex = new Regex(@"^[0-8][0-9][A-Z]{3}[0-9]{3}$");

            if (!plateRegex.IsMatch(PlateNumber))
            {
                validationResult.Errors.Add(new ValidationFailure("PlateNumber", "Invalid plate number format. Expected format: 01AAA123"));
            }
        }
        return await Task.FromResult(validationResult);
    }
}
