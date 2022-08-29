using System.Text;

namespace PollStar.Core;

public static class Randomizer
{
    private static Random random;
    private const string Pool = "abcdefghijklmnopqrstuvwxyz0123456789";

    public static string GenerateSessionCode()
    {
        var sessionCode = new StringBuilder();
        do
        {
            sessionCode.Append(Pool.Substring(random.Next(0, Pool.Length), 1));
        } while (sessionCode.Length < 6);
        return sessionCode.ToString();
    }

    static Randomizer()
    {
        random = new Random(DateTimeOffset.UtcNow.Minute *
                            DateTimeOffset.UtcNow.Second *
                            DateTimeOffset.UtcNow.Millisecond);
    }
}