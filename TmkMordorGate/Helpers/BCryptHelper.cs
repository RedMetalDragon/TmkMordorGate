namespace TmkMordorGate.Helpers;

using BCrypt.Net;

public class BCryptHelper
{
    public string HashPassword(string password)
    {
        const int workFactor = 6;
        return BCrypt.HashPassword(password, workFactor);
    }

    public string HashPassword(string password, int workFactor)
    {
        if (workFactor is < 4 or > 12)
        {
            throw new ArgumentOutOfRangeException(nameof(workFactor), "The work factor must be between 4 and 12.");
        }
        return BCrypt.HashPassword(password, workFactor);
    }

    public static bool VerifyPassword(string password, string? hashedPassword)
    {
        return !string.IsNullOrEmpty(hashedPassword) && BCrypt.Verify(password, hashedPassword);
    }
}