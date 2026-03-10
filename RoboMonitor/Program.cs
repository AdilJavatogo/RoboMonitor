using RoboMonitor.Metrics;
using RoboMonitor.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.AddServiceDefaults();

builder.Services.AddSingleton<IRobotRepository, InMemoryRobotRepository>(); // Services

builder.Services.AddSingleton<RobotMetrics>(); // Services

var app = builder.Build();

app.Services.GetRequiredService<RobotMetrics>(); // Dette tvinger instansieringen af RobotMetrics, så dine Gauges oprettes

app.MapGet("/", () => "Hej fra OpenTelemetry!");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapPrometheusScrapingEndpoint();

app.Run();
