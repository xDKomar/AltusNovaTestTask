using AltusNovaTest;
using AltusNovaTest.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Add InMemory Database
builder.Services.AddDbContext<NovaContext>(options =>
        options.UseInMemoryDatabase("AltusNovaDb"));

var app = builder.Build();

// Initialize the database
using (var scope = app.Services.CreateScope())
{
        var services = scope.ServiceProvider;
        var context = services.GetRequiredService<NovaContext>();
        DbInitializer.Initialize(context);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
        app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapEndpoints();
app.Run();