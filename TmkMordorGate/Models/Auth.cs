namespace TmkMordorGate.Models;

public class Auth
{
    public int AuthID { get; set; }
    public string? Email { get; set; }
    public int EmployeeID { get; set; }
    public string? Salt { get; set; }
    public string? PasswordHash { get; set; }
    public string? PasswordResetToken { get; set; }
    public DateTime? PasswordResetTokenExpiration { get; set; }
    public bool? KeepLoggedIn { get; set; } // Treating binary(1) as a boolean
    public string? Status { get; set; }
    
    // Navigation Property
    //public Employee Employee { get; set; }
    
    
}