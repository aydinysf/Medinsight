using System.ComponentModel.DataAnnotations;
using MedInsight.Application.Abstractions.Auth;
using MedInsight.Application.Abstractions.Repositories;
using MedInsight.Application.Common;
using MedInsight.Domain.Cases;
using MedInsight.Domain.Cases.Events;
using MedInsight.Domain.Identity;

namespace MedInsight.Application.Consultations;

/// <summary>Doktor aksiyonlarının ortak yetki çözümü: istekteki kullanıcı → doktor profili.</summary>
public sealed class DoctorActionContext(IDoctorRepository doctors, ICurrentUser currentUser)
{
    public async Task<Doctor> ResolveDoctorAsync(CancellationToken cancellationToken)
    {
        if (currentUser.Role != UserRole.Doctor)
        {
            throw new ForbiddenAccessException("Bu işlem yalnızca doktorlar içindir.");
        }

        return await doctors.GetByUserIdAsync(currentUser.UserId, cancellationToken)
            ?? throw new ForbiddenAccessException("Doktor profili bulunamadı.");
    }

    public static Consultation EnsureActiveConsultationOf(Case medicalCase, Guid consultationId, Guid doctorId)
    {
        var consultation = medicalCase.Consultations.FirstOrDefault(c => c.Id == consultationId)
            ?? throw new ForbiddenAccessException("Konsültasyon bulunamadı.");

        if (consultation.DoctorId != doctorId)
        {
            throw new ForbiddenAccessException("Bu konsültasyon size ait değil.");
        }

        return consultation;
    }
}

public sealed record AddClinicalNote([Required] [StringLength(8000, MinimumLength = 1)] string Content);

public sealed class AddClinicalNoteHandler(ICaseRepository cases, DoctorActionContext context)
{
    public async Task<Guid?> HandleAsync(Guid caseId, Guid consultationId, AddClinicalNote command, CancellationToken cancellationToken = default)
    {
        var medicalCase = await cases.GetByIdAsync(caseId, cancellationToken);
        if (medicalCase is null)
        {
            return null;
        }

        var doctor = await context.ResolveDoctorAsync(cancellationToken);
        DoctorActionContext.EnsureActiveConsultationOf(medicalCase, consultationId, doctor.Id);

        var note = medicalCase.AddClinicalNote(consultationId, doctor.Id, command.Content);
        await cases.SaveChangesAsync(cancellationToken);
        return note.Id;
    }
}

public sealed class CompleteConsultationHandler(ICaseRepository cases, DoctorActionContext context)
{
    public async Task<ConsultationDto?> HandleAsync(Guid caseId, Guid consultationId, CancellationToken cancellationToken = default)
    {
        var medicalCase = await cases.GetByIdAsync(caseId, cancellationToken);
        if (medicalCase is null)
        {
            return null;
        }

        var doctor = await context.ResolveDoctorAsync(cancellationToken);
        var consultation = DoctorActionContext.EnsureActiveConsultationOf(medicalCase, consultationId, doctor.Id);

        medicalCase.CompleteConsultation(consultationId);
        await cases.SaveChangesAsync(cancellationToken);
        return consultation.ToDto();
    }
}

public sealed record ReviewAnalysis(
    [Required] AnalysisReviewDecision Decision,
    [StringLength(4000)] string? CorrectionNotes);

public sealed class ReviewAiAnalysisHandler(ICaseRepository cases, DoctorActionContext context)
{
    public async Task<bool?> HandleAsync(Guid caseId, Guid analysisId, ReviewAnalysis command, CancellationToken cancellationToken = default)
    {
        var medicalCase = await cases.GetByIdAsync(caseId, cancellationToken);
        if (medicalCase is null)
        {
            return null;
        }

        var doctor = await context.ResolveDoctorAsync(cancellationToken);
        if (!medicalCase.Consultations.Any(c => c.DoctorId == doctor.Id && c.Status == ConsultationStatus.Active))
        {
            throw new ForbiddenAccessException("AI analizini yalnızca vakada aktif konsültasyonu olan doktor inceleyebilir.");
        }

        medicalCase.ReviewAiAnalysis(analysisId, doctor.Id, command.Decision, command.CorrectionNotes);
        await cases.SaveChangesAsync(cancellationToken);
        return true;
    }
}

public sealed record CreateTreatmentPlan(
    [Required] [StringLength(8000, MinimumLength = 3)] string Description,
    DateOnly? FollowUpDate);

public sealed class CreateTreatmentPlanHandler(ICaseRepository cases, DoctorActionContext context)
{
    public async Task<TreatmentDto?> HandleAsync(Guid caseId, Guid consultationId, CreateTreatmentPlan command, CancellationToken cancellationToken = default)
    {
        var medicalCase = await cases.GetByIdAsync(caseId, cancellationToken);
        if (medicalCase is null)
        {
            return null;
        }

        var doctor = await context.ResolveDoctorAsync(cancellationToken);
        DoctorActionContext.EnsureActiveConsultationOf(medicalCase, consultationId, doctor.Id);

        var treatment = medicalCase.CreateTreatmentPlan(consultationId, doctor.Id, command.Description, command.FollowUpDate);
        await cases.SaveChangesAsync(cancellationToken);
        return treatment.ToDto();
    }
}

/// <summary>ADR-014: doktorun manuel "ikinci görüş" talebi — risk seviyesinden bağımsız.</summary>
public sealed class RequestEscalationHandler(ICaseRepository cases, DoctorActionContext context)
{
    public async Task<bool?> HandleAsync(Guid caseId, CancellationToken cancellationToken = default)
    {
        var medicalCase = await cases.GetByIdAsync(caseId, cancellationToken);
        if (medicalCase is null)
        {
            return null;
        }

        var doctor = await context.ResolveDoctorAsync(cancellationToken);
        if (!medicalCase.Consultations.Any(c => c.DoctorId == doctor.Id && c.Status == ConsultationStatus.Active))
        {
            throw new ForbiddenAccessException("İkinci görüş talebini yalnızca vakada aktif konsültasyonu olan doktor yapabilir.");
        }

        medicalCase.SuggestEscalation(EscalationReason.DoctorRequested);
        await cases.SaveChangesAsync(cancellationToken);
        return true;
    }
}
