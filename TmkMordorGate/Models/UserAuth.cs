namespace TmkMordorGate;

public class UserAuth
{

    public readonly int AuthID;
    public readonly string? EmailAddress;
    public readonly int UserID;
    public readonly int CustomerId;
    public readonly string? Salt;
    public readonly string? PasswordHash;
    public readonly int RoleId;

    public UserAuth(int authID, string? emailAddress, int userID, int customerId, string? salt, string? passwordHash, int roleId)
    {
        AuthID = authID;
        EmailAddress = emailAddress;
        UserID = userID;
        CustomerId = customerId;
        Salt = salt;
        PasswordHash = passwordHash;
        RoleId = roleId;
    }

}
