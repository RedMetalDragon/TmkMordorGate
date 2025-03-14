using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using TmkMordorGate.Config.Interfaces;
using TmkMordorGate.Models;
using TmkMordorGate.Repositories.Interfaces;
using TmkMordorGate.Services.Authorization;
using TmkMordorGate.Services.Interfaces;

namespace TmkMordorGateTest.Mocks;

public class FakeAccessControlRepository : IAuthenticationAuthorizationRepository
{
    // Explicit implementation for IAuthenticationRepository.GetUser using emailAddress
    Task<Auth?> IAuthenticationRepository.GetUser(string emailAddress)
    {
        // For testing, return a valid Auth if emailAddress is "test@example.com".
        if (emailAddress == "rmguevara93@gmail.com")
        {
            return Task.FromResult<Auth?>(new Auth { AuthID = 123, Email = "rmguevara93@gmail.com" });
        }

        return Task.FromResult<Auth?>(null);
    }

    public Task<IEnumerable<string>> GetPermissions(int roleId)
    {
        // For testing, if roleId is 1, return a sample permission list; otherwise, return an empty list.
        if (roleId == 1)
        {
            return Task.FromResult<IEnumerable<string>>(new List<string> { "read", "write" });
        }

        return Task.FromResult<IEnumerable<string>>(new List<string>());
    }

    public Task<Feature?> GetFeature(int featureId)
    {
        // For testing, if featureId is 1, return a dummy Feature; otherwise, return null.
        if (featureId == 1)
        {
            return Task.FromResult<Feature?>(new Feature { FeatureID = 1, FeatureName = "FeatureOne" });
        }

        return Task.FromResult<Feature?>(null);
    }

    public Task<Plan?> GetPlan(int planId)
    {
        // For testing, if planId is 1, return a dummy Plan; otherwise, return null.
        if (planId == 1)
        {
            return Task.FromResult<Plan?>(new Plan { PlanID = 1, PlanName = "PlanBasic" });
        }

        return Task.FromResult<Plan?>(null);
    }

    public Task<Role?> GetRole(int roleId)
    {
        // For testing, if roleId is 1, return a dummy Role; otherwise, return null.
        if (roleId == 1)
        {
            return Task.FromResult<Role?>(new Role { RoleID = 1, RoleName = "Admin" });
        }

        return Task.FromResult<Role?>(null);
    }
}

public class FakeAuthorizationFactory : IAuthorizationFactory
{
    private IServiceProvider _serviceProvider;

    public FakeAuthorizationFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IAuthorizationService? CreateAuthorizationInstance(string className)
    {
        switch (className)
        {
            case "TmkBrain":
                var iAuthenticationAuthorizationRepository =
                    _serviceProvider.GetService<IAuthenticationAuthorizationRepository>();
                return new TmkBrain(iAuthenticationAuthorizationRepository);
            default:
                return new Dummy();
        }
    }

    public IAuthorizationService? CreateAuthorizationInstance(Func<string, bool> predicate, string className)
    {
        return CreateAuthorizationInstance(className);
    }

    public IAuthorizationService? CreateAuthorizationInstance(Func<string, bool> predicate, string className,
        string targetRoute)
    {
        return CreateAuthorizationInstance(className);
    }

    public IAuthorizationService? GetAuthorizationService(HttpContext context)
    {
        throw new NotImplementedException();
    }
}