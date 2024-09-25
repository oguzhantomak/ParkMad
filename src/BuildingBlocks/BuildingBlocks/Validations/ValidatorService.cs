namespace BuildingBlocks.Validations;

public class ValidatorService
{
    public async Task<ValidationResult> ValidateAsync<T>(T entity) where T : IValidatable
    {
        return await entity.ValidateAsync();
    }
}