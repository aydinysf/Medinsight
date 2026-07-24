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
        // Sağlayıcı seçimi ileride config'e bağlanır; MVP: deterministik stub.
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
