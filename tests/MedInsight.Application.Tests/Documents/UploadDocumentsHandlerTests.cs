using System.Text;
using MedInsight.Application.Common;
using MedInsight.Application.Documents;
using MedInsight.Application.Tests.Fakes;
using MedInsight.Domain.Cases;
using MedInsight.Domain.Identity;

namespace MedInsight.Application.Tests.Documents;

public class UploadDocumentsHandlerTests
{
    private readonly InMemoryCaseRepository _cases = new();
    private readonly FakeObjectStorage _storage = new();
    private readonly FakeCurrentUser _currentUser = new();

    private UploadDocumentsHandler Handler => new(_cases, _storage, _currentUser);

    private Case SeedCase()
    {
        var medicalCase = Case.Create(Guid.NewGuid(), _currentUser.UserId, "Vaka");
        _cases.Add(medicalCase);
        return medicalCase;
    }

    private static UploadFileInput File(string name, string content) =>
        new(name, "application/pdf", Encoding.UTF8.GetBytes(content));

    [Fact]
    public async Task Dosyalar_depolanir_ve_belgeler_olusur()
    {
        var medicalCase = SeedCase();

        var result = await Handler.HandleAsync(new UploadDocuments(medicalCase.Id, [File("a.pdf", "icerik-a"), File("b.pdf", "icerik-b")]));

        Assert.NotNull(result);
        Assert.Equal(2, result.Documents.Count);
        Assert.All(result.Documents, d => Assert.False(d.AlreadyExisted));
        Assert.Equal(2, _storage.Objects.Count);
        Assert.Equal(2, medicalCase.Documents.Count);
    }

    [Fact]
    public async Task Ayni_icerik_tekrar_gonderilirse_yeniden_islenmez_resumable()
    {
        var medicalCase = SeedCase();
        await Handler.HandleAsync(new UploadDocuments(medicalCase.Id, [File("a.pdf", "icerik-a")]));

        var retry = await Handler.HandleAsync(new UploadDocuments(medicalCase.Id, [File("a.pdf", "icerik-a"), File("b.pdf", "icerik-b")]));

        Assert.NotNull(retry);
        Assert.True(retry.Documents.Single(d => d.FileName == "a.pdf").AlreadyExisted);
        Assert.False(retry.Documents.Single(d => d.FileName == "b.pdf").AlreadyExisted);
        Assert.Equal(2, medicalCase.Documents.Count);
        Assert.Equal(2, _storage.Objects.Count);
    }

    [Fact]
    public async Task Uye_olmayan_kullanici_yukleyemez()
    {
        var medicalCase = Case.Create(Guid.NewGuid(), Guid.NewGuid(), "Baskasinin vakasi");
        _cases.Add(medicalCase);

        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            Handler.HandleAsync(new UploadDocuments(medicalCase.Id, [File("a.pdf", "icerik")])));
    }

    [Fact]
    public async Task Admin_uye_olmadan_yukleyebilir()
    {
        var medicalCase = Case.Create(Guid.NewGuid(), Guid.NewGuid(), "Vaka");
        _cases.Add(medicalCase);
        _currentUser.Role = UserRole.Admin;

        var result = await Handler.HandleAsync(new UploadDocuments(medicalCase.Id, [File("a.pdf", "icerik")]));

        Assert.NotNull(result);
        Assert.Single(result.Documents);
    }

    [Fact]
    public async Task Bos_dosya_listesi_reddedilir()
    {
        var medicalCase = SeedCase();

        await Assert.ThrowsAsync<ArgumentException>(() => Handler.HandleAsync(new UploadDocuments(medicalCase.Id, [])));
    }

    [Fact]
    public async Task Vaka_yoksa_null_doner()
    {
        Assert.Null(await Handler.HandleAsync(new UploadDocuments(Guid.NewGuid(), [File("a.pdf", "icerik")])));
    }
}
