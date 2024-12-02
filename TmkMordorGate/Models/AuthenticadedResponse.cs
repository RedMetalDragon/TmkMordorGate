namespace TmkMordorGate.Models;

public class AuthenticadedResponse(string accessToken, string emailAddress, int employeeID)
{
    public readonly string AccessToken = accessToken;
    public readonly string EmailAddress = emailAddress;
    public readonly int EmployeeId = employeeID;
}