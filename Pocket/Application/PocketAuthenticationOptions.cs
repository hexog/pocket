using System.Collections.Frozen;

namespace Pocket.Application;

public class PocketAuthenticationOptions
{
    public string[] AllowedTokenHashes { get; set; } = [];

    public FrozenSet<string> AllowedTokenHashSet { get; set; } = null!;
}