using MedInsight.Domain.Common;

namespace MedInsight.Domain.Cases;

/// <summary>
/// Hızır sohbet mesajı (ADR-018). Domain event üretmez — sohbet trafiği
/// timeline/audit'i boğmamalıdır; kalıcılık KVKK denetimi içindir.
/// İçerik column-level şifreleme teknik borcuna dahildir (security-architecture.md).
/// </summary>
public sealed class HizirChatMessage : Entity
{
    private HizirChatMessage()
    {
    }

    public Guid CaseId { get; private set; }

    /// <summary>Hızır yanıtlarında null — gönderen sistemdir.</summary>
    public Guid? SenderUserId { get; private set; }

    public bool IsFromHizir { get; private set; }

    public string Content { get; private set; } = null!;

    internal static HizirChatMessage Create(Guid caseId, Guid? senderUserId, bool isFromHizir, string content)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(content);

        return new HizirChatMessage
        {
            CaseId = caseId,
            SenderUserId = senderUserId,
            IsFromHizir = isFromHizir,
            Content = content.Trim(),
        };
    }
}
