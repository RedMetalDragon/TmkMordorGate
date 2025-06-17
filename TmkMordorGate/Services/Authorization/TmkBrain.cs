using TmkMordorGate.Repositories.Interfaces;
using TmkMordorGate.Services.Interfaces;

namespace TmkMordorGate.Services.Authorization;

public class TmkBrain: IAuthorizationService
{
    private readonly IAuthenticationAuthorizationRepository _accessControlRepository;

     public TmkBrain(IAuthenticationAuthorizationRepository accessControlRepository)
     {
         _accessControlRepository = accessControlRepository;
     }
    public async Task<bool> Authorize(HttpContext context)
    {
        // Check if the user id is in the context
        // if (!context.Items.TryGetValue("UserId", out var item))
        // {
        //     return await Task.FromResult(false);
        // }
        // Check if the request is for the brain service
        if (!context.Request.Path.HasValue || !context.Request.Path.Value.Contains("brain"))
        {
            return await Task.FromResult(false);
        }
        // Get the user id from the context
        // var userId = item?.ToString();
        // // If the user id is null, return false
        // if (userId == null)
        // {
        //     return await Task.FromResult(false);
        // }
        return await Task.FromResult(false);
        // Get the user from the repository
        // var auth = await _accessControlRepository.GetUser(userId);
        // var employeeId = auth?.EmployeeID;
        // // If the user is not found, return false
        // if (employeeId == null)
        // {
        //     return await Task.FromResult(false);
        // }
        
    }

    public string GetServiceTarget()
    {
        return "TmkBrain";
    }
}