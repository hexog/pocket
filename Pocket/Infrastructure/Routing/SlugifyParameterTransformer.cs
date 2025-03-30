using System.Text.RegularExpressions;

namespace Pocket.Infrastructure.Routing;

public partial class SlugifyParameterTransformer : IOutboundParameterTransformer
{
    public string? TransformOutbound(object? value)
    {
        return value switch
        {
            null => null,
            string str => Slugify(str),
            _ => value.ToString() is { } str ? Slugify(str) : null
        };

        static string Slugify(string value)
        {
            return SlugifyRegex.Replace(value, static match =>
            {
                Span<char> resultSpan = stackalloc char[3];
                match.Captures[0].ValueSpan.ToLowerInvariant(resultSpan);
                resultSpan[2] = resultSpan[1];
                resultSpan[1] = '-';
                return new string(resultSpan);
            }).ToLower();
        }
    }

    [GeneratedRegex("([a-z])([A-Z])", RegexOptions.None, matchTimeoutMilliseconds: 500)]
    private static partial Regex SlugifyRegex { get; }
}
