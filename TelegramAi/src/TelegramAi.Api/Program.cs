using Microsoft.EntityFrameworkCore;
using TelegramAi.Application.Interfaces;
using TelegramAi.Infrastructure.Extensions;
using TelegramAi.Infrastructure.Persistence;
using TelegramAi.TelegramBot;
using TelegramAi.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<IUserContext, HeaderUserContext>();
builder.Services.AddHostedService<TelegramBotHostedService>();

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();

    var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
    await userService.RegisterOrUpdateAsync("demo@tgassistant.dev", "Demo Admin", CancellationToken.None);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseAuthorization();
app.MapControllers();

await app.RunAsync();
