using MedInsight.Application.Auth;
using MedInsight.Application.Tests.Fakes;
using MedInsight.Domain.Identity;

namespace MedInsight.Application.Tests.Auth;

public class LoginHandlerTests
{
    private readonly InMemoryUserRepository _users = new();

    private LoginHandler Handler => new(_users, new FakePasswordHasher(), new FakeJwtTokenGenerator());

    private User SeedUser(string email = "test@example.com", string password = "parola-123")
    {
        var user = User.Create("Test Hasta", email, UserRole.Patient, new FakePasswordHasher().Hash(password));
        _users.Add(user);
        return user;
    }

    [Fact]
    public async Task Dogru_kimlik_token_doner()
    {
        var user = SeedUser();

        var result = await Handler.HandleAsync(new Login("TEST@example.com", "parola-123"));

        Assert.NotNull(result);
        Assert.Equal(user.Id, result.UserId);
        Assert.Equal(UserRole.Patient, result.Role);
        Assert.NotEmpty(result.AccessToken);
    }

    [Fact]
    public async Task Yanlis_parola_null_doner()
    {
        SeedUser();

        Assert.Null(await Handler.HandleAsync(new Login("test@example.com", "yanlis")));
    }

    [Fact]
    public async Task Olmayan_kullanici_null_doner()
    {
        Assert.Null(await Handler.HandleAsync(new Login("yok@example.com", "parola-123")));
    }

    [Fact]
    public async Task Askiya_alinmis_kullanici_giremez()
    {
        var user = SeedUser();
        user.Suspend();

        Assert.Null(await Handler.HandleAsync(new Login("test@example.com", "parola-123")));
    }
}
