namespace TmkMordorGate.Models;
using System.Text.Json.Serialization;

public class LoginRequest
{
    [JsonPropertyName("email_address")]
    public string Email { get; set; }
    
    [JsonPropertyName("password")]
    public string Password { get; set; }
}