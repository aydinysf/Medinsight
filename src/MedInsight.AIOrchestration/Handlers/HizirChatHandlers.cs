using System.ComponentModel.DataAnnotations;
using MedInsight.Application.Abstractions.Auth;
using MedInsight.Application.Abstractions.Repositories;
using MedInsight.Application.Common;
using MedInsight.Domain.Cases;
using MedInsight.Domain.Identity;

namespace MedInsight.AIOrchestration.Handlers;

public sealed record SendHizirChatMessage([Required] [StringLength(2000, MinimumLength = 1)] string Message);

public sealed record HizirChatMessageDto(Guid Id, bool IsFromHizir, string Content, DateTime CreatedAtUtc);

public static class HizirChatMappings
{
    public static HizirChatMessageDto ToDto(this HizirChatMessage message) =>
        new(message.Id, message.IsFromHizir, message.Content, message.CreatedAtUtc);
}

/// <summary>
/// ADR-018: senkron sohbet — kullanıcı mesajı + Hızır yanıtı tek transaction'da
/// kaydedilir; domain event üretilmez. Erişim: vaka üyesi veya Admin.
/// </summary>
public sealed class SendHizirChatMessageHandler(ICaseRepository cases, HizirOrchestrator orchestrator, ICurrentUser currentUser)
{
    private const int HistoryWindow = 20;

    public async Task<HizirChatMessageDto?> HandleAsync(Guid caseId, SendHizirChatMessage command, CancellationToken cancellationToken = default)
    {
        var medicalCase = await cases.GetByIdAsync(caseId, cancellationToken);
        if (medicalCase is null)
        {
            return null;
        }

        EnsureCanAccess(medicalCase, currentUser);

        // Bağlam penceresi: son 20 mesaj (maliyet ve odak için sınırlı).
        var history = medicalCase.HizirChatMessages
            .OrderBy(m => m.CreatedAtUtc)
            .TakeLast(HistoryWindow)
            .Select(m => new LlmChatTurn(m.IsFromHizir ? "assistant" : "user", m.Content))
            .ToList();

        medicalCase.AddHizirChatMessage(currentUser.UserId, isFromHizir: false, command.Message);

        var reply = await orchestrator.ChatAsync(medicalCase, history, command.Message, cancellationToken);
        var hizirMessage = medicalCase.AddHizirChatMessage(senderUserId: null, isFromHizir: true, reply);

        await cases.SaveChangesAsync(cancellationToken);
        return hizirMessage.ToDto();
    }

    internal static void EnsureCanAccess(Case medicalCase, ICurrentUser currentUser)
    {
        if (currentUser.Role == UserRole.Admin)
        {
            return;
        }

        if (medicalCase.Members.All(m => m.UserId != currentUser.UserId))
        {
            throw new ForbiddenAccessException("Bu vakada Hızır sohbetine erişim yetkiniz yok.");
        }
    }
}

public sealed class GetHizirChatMessagesQueryHandler(ICaseRepository cases, ICurrentUser currentUser)
{
    public async Task<IReadOnlyList<HizirChatMessageDto>?> HandleAsync(Guid caseId, CancellationToken cancellationToken = default)
    {
        var medicalCase = await cases.GetByIdAsync(caseId, cancellationToken);
        if (medicalCase is null)
        {
            return null;
        }

        SendHizirChatMessageHandler.EnsureCanAccess(medicalCase, currentUser);

        return medicalCase.HizirChatMessages
            .OrderBy(m => m.CreatedAtUtc)
            .Select(m => m.ToDto())
            .ToList();
    }
}
