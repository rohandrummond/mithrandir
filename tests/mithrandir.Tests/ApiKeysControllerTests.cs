using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using mithrandir.Data;
using mithrandir.Models.DTOs;

namespace mithrandir.Tests;

public class ApiKeysControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly WebApplicationFactory<Program> _factory;

    public ApiKeysControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor =
                    services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<MithrandirDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

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
            });
        });
        
        _client = _factory.CreateClient();
    }
}