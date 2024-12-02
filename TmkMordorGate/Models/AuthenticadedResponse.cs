namespace TmkMordorGate.Models;

public class AuthenticadedResponse
{
    public  string AccessToken { get; set; }
    public  string EmailAddress { get; set;}
    public  int EmployeeId { get; set; }

    public AuthenticadedResponse(string accessToken, string emailAddress, int employeeId)
    {
        AccessToken = accessToken;
        EmailAddress = emailAddress;
        EmployeeId = employeeId;
            
    }
}