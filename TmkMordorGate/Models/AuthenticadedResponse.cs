namespace TmkMordorGate.Models;

public class AuthenticadedResponse(string accessToken, string emailAddress)
{
    public readonly string AccessToken = accessToken;
    public readonly string EmailAddress = emailAddress;
}

