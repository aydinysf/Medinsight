using MedInsight.Application.Abstractions.Auth;
using MedInsight.Application.Abstractions.Repositories;
using MedInsight.Application.Cases;

namespace MedInsight.Application.Matching;

public sealed class GetDoctorMatchesQueryHandler(
    ICaseRepository cases,
    IDoctorRepository doctors,
    IUserRepository users,
    DoctorMatchingEngine engine,
    ICurrentUser currentUser)
{
    public async Task<IReadOnlyList<DoctorMatchResultDto>?> HandleAsync(Guid caseId, string? requiredSpecialty, CancellationToken cancellationToken = default)
    {
        var medicalCase = await cases.GetByIdAsync(caseId, cancellationToken);
        if (medicalCase is null)
        {
            return null;
        }

        GetCaseQueryHandler.EnsureCanAccess(medicalCase, currentUser);

        var verifiedDoctors = await doctors.GetVerifiedAsync(cancellationToken);
        var profiles = await doctors.GetReviewerProfilesAsync(verifiedDoctors.Select(d => d.Id).ToList(), cancellationToken);

        var names = new Dictionary<Guid, string>();
        foreach (var doctor in verifiedDoctors)
        {
            var user = await users.GetByIdAsync(doctor.UserId, cancellationToken);
            names[doctor.Id] = user?.FullName ?? "?";
        }

        return engine.Match(medicalCase, verifiedDoctors, profiles, names, requiredSpecialty);
    }
}
