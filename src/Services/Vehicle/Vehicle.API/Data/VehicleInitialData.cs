namespace Vehicle.API.Data;

public class VehicleInitialData : IInitialData
{
    public async Task Populate(IDocumentStore store, CancellationToken cancellation)
    {
        await using var session = store.LightweightSession();

        if (!await session.Query<Models.Vehicle>().AnyAsync(token: cancellation))
        {
            session.Store(GetPreConfiguredVehicle());
            await session.SaveChangesAsync(cancellation);
        }

    }

    private static IEnumerable<Models.Vehicle> GetPreConfiguredVehicle()
    {
        var faker = new Faker<Models.Vehicle>()
            .RuleFor(v => v.Id, f => Guid.NewGuid())
            .RuleFor(v => v.PlateNumber, f =>
            {
                var cityCode = f.Random.Number(1, 81).ToString("D2");
                var letters = f.Random.String2(3, "ABCDEFGHIJKLMNOPRSTUVYZ");
                var numbers = f.Random.Number(100, 999);
                return $"{cityCode}{letters}{numbers}";
            })
            .RuleFor(v => v.VehicleSize, f => f.PickRandom<VehicleSize>());

        return faker.Generate(100);
    }

}