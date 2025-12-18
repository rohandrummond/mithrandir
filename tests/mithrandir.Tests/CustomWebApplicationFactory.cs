using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using mithrandir.Data;
using StackExchange.Redis;

namespace mithrandir.Tests;

// Class instantiated in IClassFixture<CustomWebApplicationFactory>
// ReSharper disable once ClassNeverInstantiated.Global 
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    public const string TestIp = "127.0.0.1";                                                                                                                 

    public class FakeIpStartupFilter : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return app =>
            {
                app.Use(async (context, nextMiddleware) =>
                {
                    context.Connection.RemoteIpAddress = IPAddress.Parse("127.0.0.1");
                    await nextMiddleware();
                });
                next(app);
            };
        }
    }
    
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var dbContextDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(MithrandirDbContext));
            if (dbContextDescriptor != null)
                services.Remove(dbContextDescriptor);
                
            var optionsDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<MithrandirDbContext>));
            if (optionsDescriptor != null)
                services.Remove(optionsDescriptor);
                
            var optionsConfigDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(IDbContextOptionsConfiguration<MithrandirDbContext>));
            if (optionsConfigDescriptor != null)
                services.Remove(optionsConfigDescriptor);
                
            services.AddSingleton<IStartupFilter, FakeIpStartupFilter>();

            services.AddDbContext<MithrandirDbContext>(options =>
            {
                options.UseInMemoryDatabase("TestDb");
            });

            var sp = services.BuildServiceProvider();
            using (var scope = sp.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<MithrandirDbContext>();
                db.Database.EnsureCreated();
            }
                
            // Remove real Redis
            var redisDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(IConnectionMultiplexer));
            if (redisDescriptor != null)
                services.Remove(redisDescriptor);

            // Add a mock Redis using a library like Moq, or use a fake implementation
            services.AddSingleton<IConnectionMultiplexer>(
                _ => ConnectionMultiplexer.Connect("localhost:6379,allowAdmin=true,defaultDatabase=15")
            );
        });

        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "AdminApiKey", "test-admin-key" }
            });
        });
    }
}