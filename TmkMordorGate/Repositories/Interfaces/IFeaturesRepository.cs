using TmkMordorGate.Models;

namespace TmkMordorGate.Repositories.Interfaces;

public interface IFeaturesRepository
{
    Task<Feature?> GetFeature(int featureId);
}