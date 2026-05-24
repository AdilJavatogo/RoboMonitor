using RoboMonitor.Metrics;
using RoboMonitor.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.AddServiceDefaults();

builder.Services.AddSingleton<IRobotRepository, InMemoryRobotRepository>();

builder.Services.AddSingleton<RobotMetrics>();

var app = builder.Build();

app.Services.GetRequiredService<RobotMetrics>();

app.MapGet("/", () => "Hej fra OpenTelemetry!");

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthorization();

app.MapControllers();

app.MapPrometheusScrapingEndpoint();

app.Run();
