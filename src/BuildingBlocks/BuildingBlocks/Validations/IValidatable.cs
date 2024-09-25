namespace BuildingBlocks.Validations;

public interface IValidatable
{
    Task<ValidationResult> ValidateAsync();
}