using MedInsight.Application.Cases;
using MedInsight.Application.Common;
using MedInsight.Application.Tests.Fakes;
using MedInsight.Domain.Cases;
using MedInsight.Domain.Identity;

namespace MedInsight.Application.Tests.Cases;

public class CreateCaseHandlerTests
{
    private readonly InMemoryPatientRepository _patients = new();
    private readonly InMemoryCaseRepository _cases = new();
    private readonly FakeCurrentUser _currentUser = new();

    private CreateCaseHandler Handler => new(_patients, _cases, _currentUser);

    private Patient SeedPatientAsCurrentUser()
    {
        var patient = Patient.Create(_currentUser.UserId);
        _patients.Add(patient);
        return patient;
    }

    [Fact]
    public async Task Vaka_Draft_durumunda_olusur_ve_kaydedilir()
    {
        var patient = SeedPatientAsCurrentUser();

        var dto = await Handler.HandleAsync(new CreateCase(patient.Id, "Baş ağrısı takibi", "MR takip", BodySystem.Neuro));

        Assert.NotNull(dto);
        Assert.Equal(CaseStatus.Draft, dto.Status);
        Assert.Equal(RiskLevel.Unknown, dto.RiskLevel);
        Assert.Equal(patient.Id, dto.PatientId);
        Assert.Single(_cases.Cases);
        Assert.Equal(1, _cases.SaveCount);
    }

    [Fact]
    public async Task Hastanin_kullanicisi_vakaya_Manage_uyesi_olur()
    {
        var patient = SeedPatientAsCurrentUser();

        await Handler.HandleAsync(new CreateCase(patient.Id, "Vaka", null, null));

        var medicalCase = Assert.Single(_cases.Cases);
        var member = Assert.Single(medicalCase.Members);
        Assert.Equal(patient.UserId, member.UserId);
        Assert.Equal(CaseRole.Patient, member.Role);
        Assert.Equal(PermissionLevel.Manage, member.PermissionLevel);
    }

    [Fact]
    public async Task Hasta_yoksa_null_doner_ve_kayit_olusmaz()
    {
        var dto = await Handler.HandleAsync(new CreateCase(Guid.NewGuid(), "Vaka", null, null));

        Assert.Null(dto);
        Assert.Empty(_cases.Cases);
        Assert.Equal(0, _cases.SaveCount);
    }

    [Fact]
    public async Task Baska_hastanin_adina_vaka_acilamaz_403()
    {
        var otherPatient = Patient.Create(Guid.NewGuid());
        _patients.Add(otherPatient);

        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            Handler.HandleAsync(new CreateCase(otherPatient.Id, "Vaka", null, null)));
        Assert.Empty(_cases.Cases);
    }

    [Fact]
    public async Task Admin_baska_hasta_adina_vaka_acabilir()
    {
        var otherPatient = Patient.Create(Guid.NewGuid());
        _patients.Add(otherPatient);
        _currentUser.Role = UserRole.Admin;

        var dto = await Handler.HandleAsync(new CreateCase(otherPatient.Id, "Vaka", null, null));

        Assert.NotNull(dto);
    }

    [Fact]
    public async Task BodySystem_verilmezse_Unknown_atanir()
    {
        var patient = SeedPatientAsCurrentUser();

        var dto = await Handler.HandleAsync(new CreateCase(patient.Id, "Vaka", null, null));

        Assert.Equal(BodySystem.Unknown, dto!.BodySystem);
    }
}
