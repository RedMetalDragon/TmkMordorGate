using TmkMordorGate.Models;

namespace TmkMordorGate.Repositories.Interfaces;

public interface IPlanRepository
{
    Task<Plan?> GetPlan(int planId);
}