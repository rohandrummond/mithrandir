using Microsoft.EntityFrameworkCore;
using mithrandir.Data;

var builder = WebApplication.CreateBuilder(args);

// Get Postgres connection string from appsettings.Development.json
builder.Services.AddDbContext<MithrandirDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("MithrandirDb")));

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddSwaggerGen();

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
