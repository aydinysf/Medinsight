using MedInsight.AIOrchestration.Handlers;
using MedInsight.AIOrchestration.Pipeline;
using MedInsight.Domain.Cases.Events;
using MedInsight.Domain.Common;
using Microsoft.Extensions.DependencyInjection;

namespace MedInsight.AIOrchestration;

public static class DependencyInjection
{
    public static IServiceCollection AddAiOrchestration(this IServiceCollection services)
    {
        // TODO(llm-provider): Gerçek LLM entegrasyonu — yapılacaklar:
        //   1. ClaudeLlmClient : ILlmClient yaz (Anthropic API; model/prompt sürümünü LlmResult'ta döndür).
        //   2. Seçimi config'e bağla: "Ai:Provider" = "Stub" | "Claude" (OCR'daki Ocr:Provider deseniyle aynı).
        //   3. API anahtarı secrets manager'dan gelir, asla appsettings'e yazılmaz (security-architecture.md).
        //   Guardrails/persona/pipeline değişmez — yalnızca bu kayıt ve yeni sınıf.
        services.AddSingleton<ILlmClient, StubLlmClient>();

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
