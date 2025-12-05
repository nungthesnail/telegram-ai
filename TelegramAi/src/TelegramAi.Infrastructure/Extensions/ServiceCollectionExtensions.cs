using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TelegramAi.Application.Interfaces;
using TelegramAi.Infrastructure.LLM;
using TelegramAi.Infrastructure.Options;
using TelegramAi.Infrastructure.Payments;
using TelegramAi.Infrastructure.Persistence;
using TelegramAi.Infrastructure.Services;

namespace TelegramAi.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<DatabaseOptions>(configuration.GetSection(DatabaseOptions.SectionName));
        services.Configure<OpenAiOptions>(configuration.GetSection(OpenAiOptions.SectionName));
        services.Configure<LlmOptions>(configuration.GetSection(LlmOptions.SectionName));
        services.Configure<TelegramOptions>(configuration.GetSection(TelegramOptions.SectionName));
        services.Configure<SubscriptionOptions>(configuration.GetSection(SubscriptionOptions.SectionName));
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));

        var connectionString = configuration.GetConnectionString("Default") ??
                               configuration.GetSection(DatabaseOptions.SectionName)["ConnectionString"];

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Database connection string is not configured");
        }

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IChannelService, ChannelService>();
        services.AddScoped<IDialogService, DialogService>();
        services.AddScoped<IPostService, PostService>();
        services.AddScoped<ISubscriptionService, SubscriptionService>();
        services.AddScoped<IToolExecutor, ToolExecutor>();
        services.AddSingleton<ILlmModelService, LlmModelService>();
        services.AddSingleton<IPaymentGateway, StubPaymentGateway>();
        services.AddSingleton<IJwtTokenService, JwtTokenService>();
        services.AddLanguageModelProvider();

        return services;
    }

    private static IServiceCollection AddLanguageModelProvider(this IServiceCollection services)
    {
        services.AddScoped<ILanguageModelClient>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<LlmOptions>>();
            return options.Value.UseStub
                ? new StubLanguageModelClient()
                : new OpenAiLanguageModelClient(
                    sp.GetRequiredService<IOptions<OpenAiOptions>>(),
                    sp.GetRequiredService<IToolExecutor>());
        });

        return services;
    }
}


