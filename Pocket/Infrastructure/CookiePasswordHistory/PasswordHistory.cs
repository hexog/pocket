using MemoryPack;

namespace Pocket.Infrastructure.CookiePasswordHistory;

[MemoryPackable]
public partial record PasswordHistory(Dictionary<string, string> NotePasswords);
