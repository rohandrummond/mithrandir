using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using HealthChecks.UI.Client;
using mithrandir.Data;
using mithrandir.Services;
using mithrandir.Middleware;
using mithrandir.Options;

public partial class Program
{
    public static void Main(string[] args)
    {
        // Load .env file in development (standardizes secrets in one place)
        if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development" ||
            Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") == "Development")
        {
            var envPath = Path.Combine(Directory.GetCurrentDirectory(), "..", ".env");
            if (File.Exists(envPath))
            {
                DotNetEnv.Env.Load(envPath);
            }
        }

        var builder = WebApplication.CreateBuilder(args);

        // Postgres connection
        builder.Services.AddDbContext<MithrandirDbContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("MithrandirDb")));

        // Redis connection
        builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var connectionString = builder.Configuration.GetConnectionString("MithrandirRedis");

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Redis connection string is not configured");
            }
            
            return ConnectionMultiplexer.Connect(connectionString);
        });
        
        // Rate limiting options
        builder.Services.Configure<RateLimitOptions>(
            builder.Configuration.GetSection(RateLimitOptions.SectionName));

        // Time provider (for testing)
        builder.Services.AddSingleton(TimeProvider.System);

        // Inject API key service
        builder.Services.AddScoped<IApiKeyService, ApiKeyService>();
        builder.Services.AddScoped<IRateLimitService, RateLimitService>();

        // Health checks
        builder.Services.AddHealthChecks()
            .AddDbContextCheck<MithrandirDbContext>("database", tags: ["ready"])
            .AddRedis(
                builder.Configuration.GetConnectionString("MithrandirRedis") ??
                    throw new InvalidOperationException("Redis connection string is not configured"),
                name: "redis",
                tags: ["ready"]);

        // Framework services
        builder.Services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(
                    new System.Text.Json.Serialization.JsonStringEnumConverter());
            });
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        // Dev middleware
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        // Enable HTTPS redirection in production
        if (!app.Environment.IsDevelopment())
        {
            app.UseHttpsRedirection();
        }

        // Health check endpoints (before auth middleware so they're publicly accessible)
        app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
            Predicate = _ => false, // No checks - just confirms app is running
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready"),
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        // Register middleware
        app.UseMiddleware<RequestLoggingMiddleware>();
        app.UseMiddleware<AuthenticationMiddleware>();
        app.UseMiddleware<RateLimitingMiddleware>();

        app.UseAuthorization();
        app.MapControllers();

        app.Run();
    }
}