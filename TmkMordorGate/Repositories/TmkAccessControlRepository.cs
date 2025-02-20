using Microsoft.EntityFrameworkCore;
using TmkMordorGate.DbContext;
using TmkMordorGate.Models;
using TmkMordorGate.Repositories.Interfaces;

namespace TmkMordorGate.Repositories;

public class TmkAccessControlRepository :
    IAuthenticationAuthorizationRepository
{
    private readonly TimeKeeperDbContext _dbContext;

    public TmkAccessControlRepository(TimeKeeperDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Auth?> GetUser(string emailAddress)
    {
        try
        {
            var usr = await _dbContext.Auths.FirstOrDefaultAsync(x => x.Email == emailAddress);
            return usr;
        }
        finally
        {
            await _dbContext.DisposeAsync();
        }
    }

    public async Task<IEnumerable<string>> GetPermissions(int roleId)
    {
        var permissions = await _dbContext.Permissions
            .Where(x => x.RoleID == roleId)
            .Include(p => p.Feature)
            .ToListAsync();
        var features = permissions.Select(p => new[] { p.Feature.FeatureName });
        return features.SelectMany(x => x).Distinct();
    }

    public Task<Feature?> GetFeature(int featureId)
    {
        return _dbContext.Features.FirstOrDefaultAsync(f => f.FeatureID == featureId);
    }

    public Task<Plan?> GetPlan(int planId)
    {
        return _dbContext.Plans.FirstOrDefaultAsync(p => p.PlanID== planId);
    }

    public Task<Role?> GetRole(int roleId)
    {
        return _dbContext.Roles.FirstOrDefaultAsync(r => r.RoleID == roleId);
    }
}