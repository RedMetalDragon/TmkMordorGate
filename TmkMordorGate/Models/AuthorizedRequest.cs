namespace TmkMordorGate.Models;


/// <summary>
///   Represents a request that it's authorized or not.
/// </summary>
public class AuthorizedRequest
{
    public bool IsAuthorized { get; set; }
    public string? Message { get; set; }
}