using TmkMordorGate.Models;

namespace TmkMordorGate.Repositories.Interfaces;

public interface IAuthenticationRepository
{
    Task<Auth?>? GetUser(string emailAddress);
}