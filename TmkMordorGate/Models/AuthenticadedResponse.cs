using System.Text.Json.Serialization;

namespace TmkMordorGate.Models;

/* NOTE:
 * Properties name in json response were change to snake_case
 * to match previous tmkBrain code which by then was already using snake_case
 */
public class AuthenticadedResponse
{
    [JsonPropertyName("access_token")] public string AccessToken { get; set; }
    public string EmailAddress { get; set; }
    [JsonPropertyName("user_id")] public int UserId { get; set; }

    public AuthenticadedResponse(string accessToken, string emailAddress, int userId)
    {
        AccessToken = accessToken;
        EmailAddress = emailAddress;
        UserId = userId;
    }
}