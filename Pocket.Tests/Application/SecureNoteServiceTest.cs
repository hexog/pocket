using Microsoft.Extensions.DependencyInjection;

namespace Pocket.Application;

public class SecureNoteServiceTest : TestBase
{
    private SecureNoteService secureNoteService;

    public override void SetUp()
    {
        base.SetUp();

        secureNoteService = Services.GetRequiredService<SecureNoteService>();
    }

    [Test]
    public async Task TestCreateSecureNote()
    {
        var (content, password, noteId) = await CreateSecureNote();

        var anySecureNote = await secureNoteService.AnySecureNote(noteId);
        Assert.That(anySecureNote, Is.True);

        var canOpenSecureNote = await secureNoteService.CanOpenSecureNote(noteId, password);
        Assert.That(canOpenSecureNote, Is.True);

        var actualContent = await secureNoteService.OpenSecureNote(noteId, password);
        Assert.That(actualContent, Is.EqualTo(content));
    }

    [Test]
    public async Task TestOpenSecureNoteIncorrectPassword()
    {
        var (_, _, noteId) = await CreateSecureNote();
        const string invalidPassword = "Invalid password";

        var anySecureNote = await secureNoteService.AnySecureNote(noteId);
        Assert.That(anySecureNote, Is.True);

        var canOpenSecureNote = await secureNoteService.CanOpenSecureNote(noteId, invalidPassword);
        Assert.That(canOpenSecureNote, Is.False);

        var actualContent = await secureNoteService.OpenSecureNote(noteId, invalidPassword);
        Assert.That(actualContent, Is.Null);
    }

    [Test]
    public async Task TestOpenNonExistingNote()
    {
        const string noteId = "aa";
        const string password = "Password";

        var anySecureNote = await secureNoteService.AnySecureNote(noteId);
        Assert.That(anySecureNote, Is.False);

        var canOpenSecureNote = await secureNoteService.CanOpenSecureNote(noteId, password);
        Assert.That(canOpenSecureNote, Is.False);

        var actualContent = await secureNoteService.OpenSecureNote(noteId, password);
        Assert.That(actualContent, Is.Null);
    }

    private async Task<(string Content, string Password, string NoteId)> CreateSecureNote()
    {
        const string content = "Test message";
        const string password = "Test password";
        var noteId = await secureNoteService.CreateSecureNote(content, password);
        return (content, password, noteId);
    }
}