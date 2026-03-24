var builder = WebApplication.CreateBuilder(args);

// Tilføj YARP Reverse Proxy fra konfigurationen
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

app.UseHttpsRedirection();

// API Middleware for at håndtere API-nøgle-godkendelse
app.Use(async (context, next) =>
{
    // Vi kræver KUN api-nøgle, hvis man forsøger at ramme backend-API'et
    if (context.Request.Path.StartsWithSegments("/api"))
    {
        // 1. Tjek om requesten har headeren "X-API-KEY"
        if (!context.Request.Headers.TryGetValue("X-API-KEY", out var extractedApiKey))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("API Key mangler i headeren.");
            return;
        }

        // 2. Hent gyldige nøgler
        var validKeysSection = builder.Configuration.GetSection("RobotApiKeys").GetChildren();

        // 3. Tjek om nøglen er gyldig
        bool isKeyValid = validKeysSection.Any(k => k.Value == extractedApiKey.ToString());

        if (!isKeyValid)
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsync("Ugyldig API Key for denne robot.");
            return;
        }
    }

    // Hvis det er Grafana (roden) eller en gyldig API-anmodning, send trafikken videre
    await next();
});

// Map YARP
app.MapReverseProxy();

app.Run();