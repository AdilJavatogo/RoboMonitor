var builder = WebApplication.CreateBuilder(args);

// Tilføj YARP Reverse Proxy fra konfigurationen
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

app.UseHttpsRedirection();

// API Middleware for at håndtere API-nøgle-godkendelse
app.Use(async (context, next) =>
{
    // Tillad health checks at passere uden API-nøgle
    if (context.Request.Path.StartsWithSegments("/health"))
    {
        await next();
        return;
    }

    // 1. Tjek om requesten har headeren "X-API-KEY"
    if (!context.Request.Headers.TryGetValue("X-API-KEY", out var extractedApiKey))
    {
        context.Response.StatusCode = 401; // Unauthorized
        await context.Response.WriteAsync("API Key mangler.");
        return;
    }

    // 2. Hent alle gyldige API-nøgler fra appsettings.json
    var validKeysSection = builder.Configuration.GetSection("RobotApiKeys").GetChildren();

    // 3. Tjek om den modtagne nøgle matcher en af værdierne i vores konfiguration
    bool isKeyValid = validKeysSection.Any(k => k.Value == extractedApiKey.ToString());

    if (!isKeyValid)
    {
        context.Response.StatusCode = 403; // Forbidden
        await context.Response.WriteAsync("Ugyldig API Key.");
        return;
    }

    // 4. Hvis nøglen er godkendt, sendes anmodningen videre til YARP og dit API
    await next();
});

// Map YARP
app.MapReverseProxy();

app.Run();