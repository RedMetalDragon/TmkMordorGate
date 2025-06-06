namespace TmkMordorGate.Models;

public class Feature
{
    public int FeatureID { get; set; }
    public string FeatureName { get; set; }
    public string? Description { get; set; }
    public int PlanID { get; set; }
    public bool IsActive { get; set; }
}