using TmkMordorGate.Models;

namespace TmkMordorGate.Repositories.Interfaces;

public interface IRoleRepository
{
    Task<Role?> GetRole(int roleId);
}