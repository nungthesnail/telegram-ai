using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Telegram.Bot;
using TelegramAi.Application.Interfaces;
using TelegramAi.Infrastructure.Extensions;
using TelegramAi.Infrastructure.Options;
using TelegramAi.Infrastructure.Persistence;
using TelegramAi.TelegramBot;
using TelegramAi.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Telegram AI Assistant API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHttpContextAccessor();

builder.Services
    .AddSingleton<ITelegramPublisher, TelegramPublisher>()
    .AddSingleton<ITelegramChannelInfoProvider, TelegramChannelInfoProvider>();

// Configure JWT Authentication
var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions
{
    SecretKey = builder.Configuration["Jwt:SecretKey"] ?? "YourSuperSecretKeyThatShouldBeAtLeast32CharactersLong!",
    Issuer = builder.Configuration["Jwt:Issuer"] ?? "TelegramAi",
    Audience = builder.Configuration["Jwt:Audience"] ?? "TelegramAi"
};

if (string.IsNullOrWhiteSpace(jwtOptions.SecretKey) || jwtOptions.SecretKey.Length < 32)
{
    jwtOptions.SecretKey = "YourSuperSecretKeyThatShouldBeAtLeast32CharactersLong!";
}

builder.Services.Configure<JwtOptions>(options =>
{
    options.SecretKey = jwtOptions.SecretKey;
    options.Issuer = jwtOptions.Issuer;
    options.Audience = jwtOptions.Audience;
});

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey)),
            ValidateIssuer = false,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = false,
            ValidAudience = jwtOptions.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddScoped<IUserContext, JwtUserContext>();
builder.Services.AddHostedService<TelegramBotHostedService>();
builder.Services.AddSingleton(sp =>
{
    var tgOptions = sp.GetRequiredService<IOptionsMonitor<TelegramOptions>>();
    return new TelegramBotClient(tgOptions.CurrentValue.BotToken);
});

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

await app.RunAsync();
