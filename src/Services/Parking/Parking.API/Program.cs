using MassTransit;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// MassTransit
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<ParkingService>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["MassTransit:Host"], h =>
        {
            h.Username(builder.Configuration["MassTransit:Username"]);
            h.Password(builder.Configuration["MassTransit:Password"]);
        });

        cfg.ReceiveEndpoint("vehicle-created-queue", e =>
        {
            e.ConfigureConsumer<ParkingService>(context);
        });
    });
});

// Database Context

var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings");
var connectionStringx = Environment.GetEnvironmentVariable("ConnectionStrings__Database");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionStringx,
        sqlOptions => sqlOptions.CommandTimeout(180).EnableRetryOnFailure()));

// Dependency Injection
builder.Services.AddScoped<IParkingService, ParkingService>();
builder.Services.AddScoped<IParkingRepository, ParkingRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddSingleton<IEventPublisher, EventPublisher>();
builder.Services.AddScoped<IParkingService, ParkingService>();

// FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// Caching
builder.Services.AddMemoryCache();
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
});
builder.Host.UseSerilog();

var app = builder.Build();

// Migrations
//using (var scope = app.Services.CreateScope())
//{
//    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
//    dbContext.Database.Migrate();
//}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseGlobalExceptionMiddleware();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
