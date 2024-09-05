namespace TmkMordorGate.Models;

public class UserAuth(
    int authID,
    string? emailAddress,
    int userID,
    int customerId,
    string? salt,
    string? passwordHash,
    int roleId)
{
    public readonly int AuthID = authID;
    public readonly int CustomerId = customerId;
    public readonly string? EmailAddress = emailAddress;
    public readonly string? PasswordHash = passwordHash;
    public readonly int RoleId = roleId;
    public readonly string? Salt = salt;
    public readonly int UserID = userID;
}