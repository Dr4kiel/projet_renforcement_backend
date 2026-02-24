using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using AgentControlService.DTOs.Common;
using AgentControlService.Hubs;
using AgentControlService.Middleware;
using AgentControlService.Services;
using AgentControlService.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// ========== SERVICES CONFIGURATION ==========

builder.Services.AddControllers();

// JWT Authentication (validation only)
var jwtSecretKey = builder.Configuration["Jwt:SecretKey"]
    ?? throw new InvalidOperationException("JWT SecretKey is not configured");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "ProductionDashboard";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "ProductionDashboardClient";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey)),
        ClockSkew = TimeSpan.Zero
    };

    // Support JWT via query string for SignalR connections
    options.Events = new JwtBearerEvents
    {
        OnChallenge = async context =>
        {
            context.HandleResponse();
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(ApiErrorResponse.Unauthorized());
        },
        OnForbidden = async context =>
        {
            context.Response.StatusCode = 403;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(ApiErrorResponse.Forbidden());
        },
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs/agents"))
                context.Token = accessToken;
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(
                builder.Configuration["Cors:AllowedOrigins"]?.Split(',')
                ?? new[] { "http://localhost:5000" }
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// SignalR
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
});

// RabbitMQ
builder.Services.AddSingleton<IRabbitMQPublisher, RabbitMQPublisher>();
builder.Services.AddHostedService<RabbitMQConsumerService>();

// Services
builder.Services.AddScoped<IAgentService, AgentService>();

// ========== MIDDLEWARE PIPELINE ==========

var app = builder.Build();

app.UseMiddleware<ExceptionHandlerMiddleware>();

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapHub<AgentHub>("/hubs/agents");

app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "agent-control" }));

app.Run();
