using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using mithrandir.Data;
using mithrandir.Services;
using mithrandir.Middleware;
using mithrandir.Options;

public partial class Program
{
    public static void Main(string[] args)
    {
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

        // Register middleware
        app.UseMiddleware<RequestLoggingMiddleware>();
        app.UseMiddleware<AuthenticationMiddleware>();
        app.UseMiddleware<RateLimitingMiddleware>();

        app.UseAuthorization();
        app.MapControllers();

        app.Run();
    }
}