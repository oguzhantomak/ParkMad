using Vehicle.API.Vehicles.CreateVehicle;

var builder = WebApplication.CreateBuilder(args);


var assembly = typeof(Program).Assembly;
builder.Services.AddMediatR(config =>
{
    config.RegisterServicesFromAssembly(assembly);
    config.AddOpenBehavior(typeof(ValidationBehavior<,>));
    config.AddOpenBehavior(typeof(LoggingBehavior<,>));
});

builder.Services.AddMemoryCache();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<CreateVehicleCommandHandler>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["MassTransit:Host"], h =>
        {
            h.Username(builder.Configuration["MassTransit:Username"]);
            h.Password(builder.Configuration["MassTransit:Password"]);
        });

        cfg.ReceiveEndpoint("parking-response-queue", e =>
        {
            e.ConfigureConsumer<CreateVehicleCommandHandler>(context);
        });
    });
});

builder.Services.AddValidatorsFromAssembly(assembly);

builder.Services.AddCarter();

builder.Services.AddMarten(opts =>
{
    opts.Connection(builder.Configuration.GetConnectionString("Database")!);
}).UseLightweightSessions();

builder.Services.AddSingleton<IEventPublisher, EventPublisher>();


if (builder.Environment.IsDevelopment())
{
    builder.Services.InitializeMartenWith<VehicleInitialData>();
}

builder.Services.AddExceptionHandler<CustomExceptionHandler>();

var app = builder.Build();

app.MapCarter();

app.Run();
