using MedInsight.Application.Patients;
using MedInsight.Application.Tests.Fakes;
using MedInsight.Domain.Common;
using MedInsight.Domain.Identity;

namespace MedInsight.Application.Tests.Patients;

public class RegisterPatientHandlerTests
{
    private readonly InMemoryUserRepository _users = new();
    private readonly InMemoryPatientRepository _patients = new();

    private RegisterPatientHandler Handler => new(_users, _patients, new FakePasswordHasher());

    [Fact]
    public async Task Kullanici_ve_hasta_birlikte_olusur()
    {
        var dto = await Handler.HandleAsync(new RegisterPatient("Test Hasta", "Test.Hasta@Example.com", "parola-123", new DateOnly(1980, 4, 12), Sex.Female));

        var user = Assert.Single(_users.Users);
        var patient = Assert.Single(_patients.Patients);
        Assert.Equal(user.Id, patient.UserId);
        Assert.Equal(patient.Id, dto.Id);
        Assert.Equal(1, _patients.SaveCount);
    }

    [Fact]
    public async Task Rol_Patient_atanir_ve_parola_hashlenir()
    {
        await Handler.HandleAsync(new RegisterPatient("Test Hasta", "x@example.com", "parola-123", null, null));

        var user = Assert.Single(_users.Users);
        Assert.Equal(UserRole.Patient, user.Role);
        Assert.Equal("HASH::parola-123", user.PasswordHash);
    }

    [Fact]
    public async Task Eposta_kucuk_harfe_normalize_edilir()
    {
        var dto = await Handler.HandleAsync(new RegisterPatient("Test Hasta", "Test.Hasta@Example.com", "parola-123", null, null));

        Assert.Equal("test.hasta@example.com", dto.Email);
    }

    [Fact]
    public async Task Ayni_eposta_ikinci_kez_kayit_olamaz()
    {
        await Handler.HandleAsync(new RegisterPatient("Birinci", "ayni@example.com", "parola-123", null, null));

        await Assert.ThrowsAsync<DomainException>(() =>
            Handler.HandleAsync(new RegisterPatient("İkinci", "AYNI@example.com", "parola-456", null, null)));
        Assert.Single(_users.Users);
    }

    [Fact]
    public async Task Sex_verilmezse_Unknown_atanir()
    {
        var dto = await Handler.HandleAsync(new RegisterPatient("Test Hasta", "x@example.com", "parola-123", null, null));

        Assert.Equal(Sex.Unknown, dto.Sex);
    }
}
