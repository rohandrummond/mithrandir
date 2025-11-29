using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using mithrandir.Data;
using mithrandir.Services;
using mithrandir.Middleware;

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

// Inject API key service
builder.Services.AddScoped<IApiKeyService, ApiKeyService>();
builder.Services.AddScoped<IRateLimitService, RateLimitService>();

// Framework services
builder.Services.AddControllers();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Dev middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// HTTP request pipeline
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Register API key middleware
app.UseMiddleware<AuthenticationMiddleware>();
app.UseMiddleware<RateLimitingMiddleware>();

app.Run();