using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Simando.Infrastructure.Identity;

// Generates the one-time password shown once on user create / admin reset
// (docs/design/frontend/10-admin.md "Tambah Pengguna"). RandomNumberGenerator,
// never Random/Guid, for anything password-shaped. Reads the configured
// Auth:Password policy so a generated password always satisfies it —
// guarantees at least one upper/lower/digit char since RequireUppercase/
// RequireLowercase/RequireDigit default true in this project; no symbol
// requirement since RequireNonAlphanumeric is false.
public sealed class TemporaryPasswordGenerator(IOptions<IdentityOptions> identityOptions)
{
    private const string Uppercase = "ABCDEFGHJKLMNPQRSTUVWXYZ"; // no I/O — visually ambiguous
    private const string Lowercase = "abcdefghijkmnopqrstuvwxyz"; // no l
    private const string Digits = "23456789"; // no 0/1

    public string Generate()
    {
        var policy = identityOptions.Value.Password;
        var length = Math.Max(policy.RequiredLength, 12);

        var chars = new List<char> { Pick(Uppercase), Pick(Lowercase), Pick(Digits) };
        var pool = Uppercase + Lowercase + Digits;
        while (chars.Count < length)
        {
            chars.Add(Pick(pool));
        }

        Shuffle(chars);
        return new string(chars.ToArray());
    }

    private static char Pick(string alphabet) => alphabet[RandomNumberGenerator.GetInt32(alphabet.Length)];

    private static void Shuffle(List<char> chars)
    {
        for (var i = chars.Count - 1; i > 0; i--)
        {
            var j = RandomNumberGenerator.GetInt32(i + 1);
            (chars[i], chars[j]) = (chars[j], chars[i]);
        }
    }
}
