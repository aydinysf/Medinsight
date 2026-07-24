using MedInsight.Application.Abstractions.Auth;
using MedInsight.Application.Abstractions.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace MedInsight.Api.Hubs;

/// <summary>
/// Gerçek zamanlı konsültasyon mesajlaşması (consultation-model.md).
/// REST endpoint'i geçmiş sorgusu ve fallback içindir; canlı akış buradan yürür.
/// </summary>
[Authorize]
public sealed class ConsultationHub(ICaseRepository cases, ICurrentUser currentUser) : Hub
{
    public static string GroupName(Guid consultationId) => $"consultation-{consultationId}";

    /// <summary>Gruba katılım vaka üyeliğine tabidir — rol doğru olsa bile üye olmayan giremez.</summary>
    public async Task JoinConsultation(Guid caseId, Guid consultationId)
    {
        var medicalCase = await cases.GetByIdAsync(caseId, Context.ConnectionAborted);
        if (medicalCase is null
            || medicalCase.Consultations.All(c => c.Id != consultationId)
            || (currentUser.Role != Domain.Identity.UserRole.Admin
                && medicalCase.Members.All(m => m.UserId != currentUser.UserId)))
        {
            throw new HubException("Bu konsültasyona erişim yetkiniz yok.");
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(consultationId));
    }

    public Task LeaveConsultation(Guid consultationId) =>
        Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName(consultationId));
}
