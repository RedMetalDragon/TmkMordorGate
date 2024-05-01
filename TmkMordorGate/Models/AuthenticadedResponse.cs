namespace TmkMordorGate.Models;

public class AuthenticadedResponse(int auth_UserID, string? auth_EmailAddress, int auth_CustomerID, string? auth_Role, string auth_Token)
{
    public readonly int Auth_UserID = auth_UserID;
    public readonly string? Auth_EmailAddress = auth_EmailAddress;
    public readonly int Auth_CustomerID = auth_CustomerID;
    public readonly string? Auth_Role = auth_Role;
    public readonly string Auth_Token = auth_Token;

}
