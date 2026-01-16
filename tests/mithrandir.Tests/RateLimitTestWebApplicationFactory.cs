using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using mithrandir.Options;
using StackExchange.Redis;

namespace mithrandir.Tests;

public class RateLimitTestWebApplicationFactory : CustomWebApplicationFactory
{
    public FakeTimeProvider FakeTimeProvider { get; } = new FakeTimeProvider();
    public int TestRateLimit { get; set; } = 3;
    public int TestWindowMinutes { get; set; } = 10;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.ConfigureServices(services =>
        {
            // Swap in fake time provider
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(TimeProvider));
            if (descriptor != null)
                services.Remove(descriptor);

            services.AddSingleton<TimeProvider>(FakeTimeProvider);

            // Use a separate Redis database (14) to isolate rate limit tests from other tests
            var redisDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IConnectionMultiplexer));
            if (redisDescriptor != null)
                services.Remove(redisDescriptor);

            var redis = ConnectionMultiplexer.Connect("localhost:6379,allowAdmin=true,defaultDatabase=14");
            redis.GetServer("localhost:6379").FlushDatabase(14);
            services.AddSingleton<IConnectionMultiplexer>(redis);

            // Update rate limits for testing
            services.Configure<RateLimitOptions>(options =>
            {
                options.FreeTierLimit = TestRateLimit;
                options.ProTierLimit = TestRateLimit;
                options.WindowMinutes = TestWindowMinutes;
            });
        });
    }
}
