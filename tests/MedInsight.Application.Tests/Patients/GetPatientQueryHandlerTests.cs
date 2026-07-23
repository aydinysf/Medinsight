using MedInsight.Application.Patients;
using MedInsight.Application.Tests.Fakes;
using MedInsight.Domain.Identity;

namespace MedInsight.Application.Tests.Patients;

public class GetPatientQueryHandlerTests
{
    private readonly InMemoryUserRepository _users = new();
    private readonly InMemoryPatientRepository _patients = new();

    private GetPatientQueryHandler Handler => new(_patients, _users);

    [Fact]
    public async Task Hasta_ve_kullanici_bilgisi_birlesik_doner()
    {
        var user = User.Create("Test Hasta", "test@example.com");
        var patient = Patient.Create(user.Id, new DateOnly(1980, 4, 12), Sex.Female);
        _users.Add(user);
        _patients.Add(patient);

        var dto = await Handler.HandleAsync(patient.Id);

        Assert.NotNull(dto);
        Assert.Equal("Test Hasta", dto.FullName);
        Assert.Equal("test@example.com", dto.Email);
        Assert.Equal(new DateOnly(1980, 4, 12), dto.DateOfBirth);
    }

    [Fact]
    public async Task Hasta_yoksa_null_doner()
    {
        var dto = await Handler.HandleAsync(Guid.NewGuid());

        Assert.Null(dto);
    }
}
