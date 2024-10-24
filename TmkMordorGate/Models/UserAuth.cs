namespace TmkMordorGate.Models;

public class UserAuth(
    int authId,
    string? emailAddress,
    int userId,
    int customerId,
    string? salt,
    string? passwordHash,
    int roleId)
{
    public readonly int AuthId = authId;
    public readonly int CustomerId = customerId;
    public readonly string? EmailAddress = emailAddress;
    public readonly string? PasswordHash = passwordHash;
    public readonly int RoleId = roleId;
    public readonly string? Salt = salt;
    public readonly int UserId = userId;
}