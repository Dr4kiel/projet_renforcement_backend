var builder = WebApplication.CreateBuilder(args);

// ========== SERVICES CONFIGURATION ==========

// Add YARP Reverse Proxy
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// CORS Configuration
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(
                builder.Configuration["Cors:AllowedOrigins"]?.Split(',')
                ?? new[] { "http://localhost:5173", "http://localhost:3000" }
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// ========== MIDDLEWARE PIPELINE ==========

var app = builder.Build();

// Enable CORS
app.UseCors();

// Map YARP Reverse Proxy
app.MapReverseProxy();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "gateway" }));

app.Run();
