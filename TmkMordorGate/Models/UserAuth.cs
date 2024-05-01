namespace TmkMordorGate.Models;

public class UserAuth(int authID, string? emailAddress, int userID, int customerId, string? salt, string? passwordHash, int roleId)
{

    public readonly int AuthID = authID;
    public readonly string? EmailAddress = emailAddress;
    public readonly int UserID = userID;
    public readonly int CustomerId = customerId;
    public readonly string? Salt = salt;
    public readonly string? PasswordHash = passwordHash;
    public readonly int RoleId = roleId;
}
