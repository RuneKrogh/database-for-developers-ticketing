using Dapper;
using TicketingService.Application;
using TicketingService.Application.Strategies;
using TicketingService.Infrastructure.Database;
using TicketingService.Infrastructure.Repositories;

DefaultTypeMap.MatchNamesWithUnderscores = true;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSingleton<IDbConnectionFactory, NpgsqlConnectionFactory>();
builder.Services.AddScoped<DatabaseInitializer>();
builder.Services.AddScoped<EventRepository>();
builder.Services.AddScoped<BookingRepository>();
builder.Services.AddScoped<EventService>();
builder.Services.AddScoped<BookingService>();
builder.Services.AddKeyedScoped<IBookingStrategy, UnsafeBookingStrategy>("unsafe");
builder.Services.AddKeyedScoped<IBookingStrategy, PessimisticBookingStrategy>("pessimistic");

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    await scope.ServiceProvider
        .GetRequiredService<DatabaseInitializer>()
        .InitializeAsync();
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();
