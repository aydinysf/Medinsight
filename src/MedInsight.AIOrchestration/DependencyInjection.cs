using MedInsight.AIOrchestration.Handlers;
using MedInsight.AIOrchestration.Pipeline;
using MedInsight.Domain.Cases.Events;
using MedInsight.Domain.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MedInsight.AIOrchestration;

public static class DependencyInjection
{
    public static IServiceCollection AddAiOrchestration(this IServiceCollection services, IConfiguration configuration)
    {
        // Sağlayıcı seçimi config'ten (Ocr:Provider deseniyle aynı). API anahtarları
        // user-secrets / secrets manager'dan gelir, asla appsettings'e yazılmaz.
        // Yeni sağlayıcı (örn. ClaudeLlmClient) eklemek = yeni ILlmClient sınıfı + buraya bir dal;
        // guardrails/persona/pipeline değişmez.
        var provider = configuration["Ai:Provider"] ?? "Stub";
        if (string.Equals(provider, "Gemini", StringComparison.OrdinalIgnoreCase))
        {
            services.Configure<GeminiOptions>(configuration.GetSection(GeminiOptions.SectionName));
            services.AddHttpClient<ILlmClient, GeminiLlmClient>(client => client.Timeout = TimeSpan.FromSeconds(100));
        }
        else if (provider.ToLowerInvariant() is "kimi" or "deepseek" or "openaicompatible")
        {
            services.Configure<OpenAiCompatibleOptions>(configuration.GetSection(OpenAiCompatibleOptions.SectionName));
            services.PostConfigure<OpenAiCompatibleOptions>(opts =>
            {
                // Sağlayıcı adına göre varsayılanlar — config doldurulmuşsa dokunulmaz.
                if (string.IsNullOrWhiteSpace(opts.BaseUrl))
                {
                    opts.BaseUrl = provider.ToLowerInvariant() switch
                    {
                        "kimi" => "https://api.moonshot.ai/v1",
                        "deepseek" => "https://api.deepseek.com/v1",
                        _ => opts.BaseUrl,
                    };
                }

                if (string.IsNullOrWhiteSpace(opts.Model))
                {
                    opts.Model = provider.ToLowerInvariant() switch
                    {
                        "kimi" => "kimi-latest",
                        "deepseek" => "deepseek-chat",
                        _ => opts.Model,
                    };
                }
            });
            services.AddHttpClient<ILlmClient, OpenAiCompatibleLlmClient>(client => client.Timeout = TimeSpan.FromSeconds(100));
        }
        else
        {
            services.AddSingleton<ILlmClient, StubLlmClient>();
        }

        services.AddSingleton<IntentDetector>();
        services.AddSingleton<AnalysisPlanner>();
        services.AddSingleton<AgentSelector>();
        services.AddSingleton<CaseToolInvoker>();
        services.AddSingleton<MemoryContextBuilder>();
        services.AddSingleton<ReasoningEngine>();
        services.AddSingleton<Guardrails>();
        services.AddSingleton<ResponseComposer>();
        services.AddSingleton<HizirOrchestrator>();

        services.AddScoped<IDomainEventHandler<AIAnalysisRequested>, OnAIAnalysisRequested>();
        services.AddScoped<IDomainEventHandler<AIAnalysisCompleted>, OnAIAnalysisCompletedConfidenceCheck>();

        return services;
    }
}
