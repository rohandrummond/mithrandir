using Microsoft.EntityFrameworkCore;
using mithrandir.Data;
using mithrandir.Services;

var builder = WebApplication.CreateBuilder(args);

// Get Postgres connection string from appsettings.Development.json
builder.Services.AddDbContext<MithrandirDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("MithrandirDb")));

// Add services 
builder.Services.AddControllers();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IApiKeyService, ApiKeyService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
