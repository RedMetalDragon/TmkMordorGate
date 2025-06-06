namespace TmkMordorGate.Repositories.Interfaces;

public interface IAuthenticationAuthorizationRepository: 
    IAuthenticationRepository,
    IPermissionRepository,
    IFeaturesRepository,
    IPlanRepository,
    IRoleRepository
{
    
}