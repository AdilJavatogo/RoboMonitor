var builder = WebApplication.CreateBuilder(args);

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/api/RobotData", StringComparison.OrdinalIgnoreCase))
    {
        if (!context.Request.Headers.TryGetValue("X-API-KEY", out var extractedApiKey))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("API Key mangler i headeren.");
            return;
        }

        var validKeysSection = builder.Configuration.GetSection("RobotApiKeys").GetChildren();

        bool isKeyValid = validKeysSection.Any(k => k.Value == extractedApiKey.ToString());

        if (!isKeyValid)
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsync("Ugyldig API Key for denne robot.");
            return;
        }
    }

    await next();
});

app.MapReverseProxy();

app.Run();