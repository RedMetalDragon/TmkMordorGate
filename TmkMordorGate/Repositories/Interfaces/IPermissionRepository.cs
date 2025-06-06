using TmkMordorGate.Models;

namespace TmkMordorGate.Repositories.Interfaces;

public interface IPermissionRepository
{
    Task<IEnumerable<string>> GetPermissions(int roleId);
}