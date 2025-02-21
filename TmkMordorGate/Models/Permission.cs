namespace TmkMordorGate.Models;

public class Permission
{
    public int PermissionID { get; set; }
    public int RoleID { get; set; }
    public int FeatureID { get; set; }

    public Role Role { get; set; }
    public Feature Feature { get; set; }
}