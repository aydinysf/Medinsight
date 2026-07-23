using MedInsight.Application.Common;
using MedInsight.Application.Patients;
using MedInsight.Application.Tests.Fakes;
using MedInsight.Domain.Identity;

namespace MedInsight.Application.Tests.Patients;

public class GetPatientQueryHandlerTests
{
    private readonly InMemoryUserRepository _users = new();
    private readonly InMemoryPatientRepository _patients = new();
    private readonly FakeCurrentUser _currentUser = new();

    private GetPatientQueryHandler Handler => new(_patients, _users, _currentUser);

    private (User User, Patient Patient) Seed()
    {
        var user = User.Create("Test Hasta", "test@example.com", UserRole.Patient, "hash");
        var patient = Patient.Create(user.Id, new DateOnly(1980, 4, 12), Sex.Female);
        _users.Add(user);
        _patients.Add(patient);
        return (user, patient);
    }

    [Fact]
    public async Task Hasta_kendini_gorebilir()
    {
        var (user, patient) = Seed();
        _currentUser.UserId = user.Id;

        var dto = await Handler.HandleAsync(patient.Id);

        Assert.NotNull(dto);
        Assert.Equal("Test Hasta", dto.FullName);
        Assert.Equal(new DateOnly(1980, 4, 12), dto.DateOfBirth);
    }

    [Fact]
    public async Task Baskasi_goremez_403()
    {
        var (_, patient) = Seed();

        await Assert.ThrowsAsync<ForbiddenAccessException>(() => Handler.HandleAsync(patient.Id));
    }

    [Fact]
    public async Task Admin_herkesi_gorebilir()
    {
        var (_, patient) = Seed();
        _currentUser.Role = UserRole.Admin;

        var dto = await Handler.HandleAsync(patient.Id);

        Assert.NotNull(dto);
    }

    [Fact]
    public async Task Hasta_yoksa_null_doner()
    {
        _currentUser.Role = UserRole.Admin;

        var dto = await Handler.HandleAsync(Guid.NewGuid());

        Assert.Null(dto);
    }
}
