using System.Security.Cryptography;
using System.Text;
using Geralt;
using Microsoft.EntityFrameworkCore;
using Pocket.Data;
using Pocket.Data.Models;

namespace Pocket.Application;

public class SecureNoteService(
    ApplicationDbContext dbContext,
    IClock clock
)
{
    private static readonly byte[] EmptyNonce = new byte[12];

    public async Task<bool> AnySecureNote(string noteId)
    {
        return await dbContext.SecureNotes.AnyAsync(x => x.PublicId == noteId);
    }

    public async Task<string> CreateSecureNote(string content, string password)
    {
        const string alphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_,=";
        var secureNoteId = RandomNumberGenerator.GetString(alphabet, 20);

        var salt = RandomNumberGenerator.GetBytes(16);
        var key = DeriveKey(password, salt);

        var keyHash = ComputeHash(key);
        var plaintext = Encoding.UTF8.GetBytes(content);
        var ciphertext = new byte[plaintext.Length];
        ChaCha20.Encrypt(
            ciphertext,
            plaintext,
            EmptyNonce,
            key
        );

        dbContext.SecureNotes.Add(
            new SecureNoteEntity
            {
                Content = ciphertext,
                Salt = salt,
                CreatedAt = clock.GetCurrentInstant(),
                KeyHash = keyHash,
                PublicId = secureNoteId
            }
        );

        await dbContext.SaveChangesAsync();
        return secureNoteId;
    }

    public async Task<bool> CanOpenSecureNote(string noteId, string password)
    {
        var secureNote = await dbContext.SecureNotes
            .Where(x => x.PublicId == noteId)
            .Select(x => new { x.Salt, x.KeyHash })
            .FirstOrDefaultAsync();

        if (secureNote is null)
        {
            return false;
        }

        var salt = secureNote.Salt;
        var key = DeriveKey(password, salt);

        var keyHash = ComputeHash(key);
        if (!ConstantTime.Equals(keyHash, secureNote.KeyHash))
        {
            return false;
        }

        return true;
    }

    public async Task<string?> OpenSecureNote(string noteId, string password)
    {
        var secureNote = await dbContext.SecureNotes
            .Where(x => x.PublicId == noteId)
            .Select(x => new { x.Id, x.Salt, x.KeyHash })
            .FirstOrDefaultAsync();

        if (secureNote is null)
        {
            return null;
        }

        var salt = secureNote.Salt;
        var key = DeriveKey(password, salt);

        var keyHash = ComputeHash(key);
        if (!ConstantTime.Equals(keyHash, secureNote.KeyHash))
        {
            return null;
        }

        var secureNoteId = secureNote.Id;
        var content = await dbContext.SecureNotes.Where(x => x.Id == secureNoteId).Select(x => x.Content).FirstAsync();

        var plaintext = new byte[content.Length];
        ChaCha20.Decrypt(plaintext, content, EmptyNonce, key);

        return Encoding.UTF8.GetString(plaintext);
    }

    private static byte[] ComputeHash(byte[] key)
    {
        var hash = new byte[64];
        BLAKE2b.ComputeHash(hash, key);
        return hash;
    }

    private static byte[] DeriveKey(string password, byte[] salt)
    {
        var key = new byte[32];
        Argon2id.DeriveKey(
            key,
            Encoding.UTF8.GetBytes(password),
            salt,
            iterations: 3,
            memorySize: 67108864
        );

        return key;
    }
}