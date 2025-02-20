using TmkMordorGate.Services.Interfaces;

namespace TmkMordorGate.Config.Interfaces;

public interface IAuthorizationFactory
{
    /// <summary>
    ///  Creates an authorization service based on the provided class name.
    /// </summary>
    /// <param name="className">
    ///  The name of the class to be used for authorization.
    /// </param>
    /// <returns></returns>
    public IAuthorizationService? CreateAuthorizationService(string className);

    /// <summary>
    /// Creates an authorization service based on the provided predicate and class name.
    /// </summary>
    /// <param name="predicate">
    ///  The predicate function to determine if the class name matches the desired criteria.
    /// </param>
    /// <param name="className">
    ///   The name of the class to be used for authorization.
    /// </param>
    /// <returns></returns>
    public IAuthorizationService? CreateAuthenticationService(Func<string, bool> predicate, string className);
}