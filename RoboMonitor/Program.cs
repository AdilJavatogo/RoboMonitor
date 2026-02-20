using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

//var serviceName = "RobotMonitor";

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

//builder.Services.AddOpenTelemetry()
//    .ConfigureResource(resource => resource
//        .AddService(serviceName: builder.Environment.ApplicationName))

//    .WithTracing(tracing =>
//            {
//                tracing
//                .AddAspNetCoreInstrumentation()
//                .AddHttpClientInstrumentation()
//                .AddSqlClientInstrumentation();

//            })

//    .WithMetrics(metrics =>
//           {
//               metrics
//               .AddAspNetCoreInstrumentation()
//               .AddHttpClientInstrumentation()
//               .AddRuntimeInstrumentation();
//               //.AddPrometheusExporter();

//           });


builder.AddServiceDefaults();


var app = builder.Build();

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
