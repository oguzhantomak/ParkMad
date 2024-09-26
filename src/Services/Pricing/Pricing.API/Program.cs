using BuildingBlocks.Exceptions;
using MassTransit;
using Pricing.API.Consumers;
using Pricing.API.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// MassTransit
builder.Services.AddMassTransit(x =>
{
    // PricingRequestConsumer'ý ekliyoruz
    x.AddConsumer<PricingRequestConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["MassTransit:Host"], h =>
        {
            h.Username(builder.Configuration["MassTransit:Username"]);
            h.Password(builder.Configuration["MassTransit:Password"]);
        });

        // PricingRequestEvent'leri dinleyen bir endpoint oluþturuyoruz
        cfg.ReceiveEndpoint("pricing-request-queue", e =>
        {
            e.ConfigureConsumer<PricingRequestConsumer>(context);
        });
    });
});

// Dependency Injection
builder.Services.AddScoped<IPriceStrategyFactory, PriceStrategyFactory>();
builder.Services.AddScoped<IPricingService, PricingService>();

builder.Services.AddScoped<PricingRequestConsumer>();


builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = Environment.GetEnvironmentVariable("ConnectionStrings__Redis");
    options.InstanceName = "ParkingAppRedisInstance";
});

// Logging
builder.Host.UseSerilog();

var app = builder.Build();

// Middleware
app.UseGlobalExceptionMiddleware();
app.Run();
