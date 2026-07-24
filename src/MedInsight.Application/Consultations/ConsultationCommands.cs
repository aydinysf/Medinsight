using System.ComponentModel.DataAnnotations;
using MedInsight.Application.Abstractions.Auth;
using MedInsight.Application.Abstractions.Repositories;
using MedInsight.Application.Cases;
using MedInsight.Application.Common;
using MedInsight.Domain.Cases;
using MedInsight.Domain.Common;
using MedInsight.Domain.Identity;

namespace MedInsight.Application.Consultations;

public sealed record ConsultationDto(Guid Id, Guid CaseId, Guid DoctorId, ConsultationStatus Status, DateTime StartedAtUtc, DateTime? CompletedAtUtc);

public sealed record ConsultationMessageDto(Guid Id, Guid ConsultationId, Guid SenderUserId, string Content, DateTime SentAtUtc);

public sealed record TreatmentDto(Guid Id, Guid CaseId, Guid ConsultationId, Guid CreatedByDoctorId, string Description, DateOnly? FollowUpDate, DateTime CreatedAtUtc);

public static class ConsultationMappings
{
    public static ConsultationDto ToDto(this Consultation consultation) =>
        new(consultation.Id, consultation.CaseId, consultation.DoctorId, consultation.Status, consultation.StartedAtUtc, consultation.CompletedAtUtc);

    public static ConsultationMessageDto ToDto(this ConsultationMessage message) =>
        new(message.Id, message.ConsultationId, message.SenderUserId, message.Content, message.CreatedAtUtc);

    public static TreatmentDto ToDto(this Treatment treatment) =>
        new(treatment.Id, treatment.CaseId, treatment.ConsultationId, treatment.CreatedByDoctorId, treatment.Description, treatment.FollowUpDate, treatment.CreatedAtUtc);
}

public sealed record StartConsultation([Required] Guid DoctorId);

/// <summary>Hasta (Manage üyesi) veya admin doktoru vakaya davet eder — Doctor Matching önerir, seçim hastanındır.</summary>
public sealed class StartConsultationHandler(ICaseRepository cases, IDoctorRepository doctors, ICurrentUser currentUser)
{
    public async Task<ConsultationDto?> HandleAsync(Guid caseId, StartConsultation command, CancellationToken cancellationToken = default)
    {
        var medicalCase = await cases.GetByIdAsync(caseId, cancellationToken);
        if (medicalCase is null)
        {
            return null;
        }

        EnsureManageAccess(medicalCase, currentUser);

        var doctor = await doctors.GetByIdAsync(command.DoctorId, cancellationToken)
            ?? throw new DomainException("Doktor bulunamadı.");
        if (doctor.VerificationStatus != VerificationStatus.Verified)
        {
            throw new DomainException("Yalnızca doğrulanmış doktorlarla konsültasyon başlatılabilir (ADR-007).");
        }

        var consultation = medicalCase.StartConsultation(doctor.Id, doctor.UserId);
        await cases.SaveChangesAsync(cancellationToken);
        return consultation.ToDto();
    }

    internal static void EnsureManageAccess(Case medicalCase, ICurrentUser currentUser)
    {
        if (currentUser.Role == UserRole.Admin)
        {
            return;
        }

        var member = medicalCase.Members.FirstOrDefault(m => m.UserId == currentUser.UserId);
        if (member is null || member.PermissionLevel < PermissionLevel.Manage)
        {
            throw new ForbiddenAccessException("Vakaya doktor ekleme yetkiniz yok.");
        }
    }
}

public sealed record SendConsultationMessage([Required] [StringLength(4000, MinimumLength = 1)] string Content);

public sealed class SendConsultationMessageHandler(ICaseRepository cases, ICurrentUser currentUser)
{
    public async Task<ConsultationMessageDto?> HandleAsync(Guid caseId, Guid consultationId, SendConsultationMessage command, CancellationToken cancellationToken = default)
    {
        var medicalCase = await cases.GetByIdAsync(caseId, cancellationToken);
        if (medicalCase is null)
        {
            return null;
        }

        GetCaseQueryHandler.EnsureCanAccess(medicalCase, currentUser);

        var message = medicalCase.AddConsultationMessage(consultationId, currentUser.UserId, command.Content);
        await cases.SaveChangesAsync(cancellationToken);
        return message.ToDto();
    }
}

public sealed class GetConsultationMessagesQueryHandler(ICaseRepository cases, ICurrentUser currentUser)
{
    public async Task<IReadOnlyList<ConsultationMessageDto>?> HandleAsync(Guid caseId, Guid consultationId, CancellationToken cancellationToken = default)
    {
        var medicalCase = await cases.GetByIdAsync(caseId, cancellationToken);
        if (medicalCase is null || medicalCase.Consultations.All(c => c.Id != consultationId))
        {
            return null;
        }

        GetCaseQueryHandler.EnsureCanAccess(medicalCase, currentUser);

        var messages = await cases.GetConsultationMessagesAsync(consultationId, cancellationToken);
        return messages.Select(m => m.ToDto()).ToList();
    }
}

public sealed class GetCaseConsultationsQueryHandler(ICaseRepository cases, ICurrentUser currentUser)
{
    public async Task<IReadOnlyList<ConsultationDto>?> HandleAsync(Guid caseId, CancellationToken cancellationToken = default)
    {
        var medicalCase = await cases.GetByIdAsync(caseId, cancellationToken);
        if (medicalCase is null)
        {
            return null;
        }

        GetCaseQueryHandler.EnsureCanAccess(medicalCase, currentUser);
        return medicalCase.Consultations.OrderByDescending(c => c.StartedAtUtc).Select(c => c.ToDto()).ToList();
    }
}
