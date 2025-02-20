using System.Collections.Immutable;
using System.Reflection;
using System.Globalization;
using TmkMordorGate.Config.Interfaces;
using TmkMordorGate.Services.Interfaces;

namespace TmkMordorGate.Helpers;

public sealed class AuthorizationFactory : IAuthorizationFactory
{
    private readonly ImmutableDictionary<string, Type> _authTypes;

    public AuthorizationFactory()
    {
        _authTypes = FindAuthorizationServiceTypes(CultureInfo.CurrentCulture);
    }

    private static ImmutableDictionary<string, Type> FindAuthorizationServiceTypes(CultureInfo cultureInfo, bool ignoreCase = true) 
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        var authTypes = new Dictionary<string, Type>();

        foreach (var assembly in assemblies)
        {
            try
            {
                // First, check if the assembly contains any types implementing IAuthorizationService
                var hasAuthServices = assembly.GetTypes()
                    .Any(t => t.GetInterfaces()
                        .Contains(typeof(IAuthorizationService)));

                if (!hasAuthServices)
                    continue;

                // Then process the assembly's types
                var validTypes = assembly.GetTypes()
                    .Where(IsValidAuthorizationType)
                    .Where(t => !t.Name.StartsWith("<")); // Filter out compiler-generated types

                foreach (var type in validTypes)
                {
                    authTypes.TryAdd(type.Name, type);
                }
            }
            catch (ReflectionTypeLoadException)
            {
                // Skip assemblies that can't be loaded
                continue;
            }
        }
        return authTypes.ToImmutableDictionary();
    }

    public void PrintTypes()
    {
        Console.WriteLine("--------Authorization types--------");
        foreach (var type in _authTypes.Keys)
        {
            Console.WriteLine(type);
        }
        Console.WriteLine("-----------------------------------");
    }
    
    public IAuthorizationService? CreateAuthorizationService(string className)
    {
        try
        {
            if (!_authTypes.TryGetValue(className, out var type))
            {
                return null;
            }

            return (IAuthorizationService)Activator.CreateInstance(type)!;
        }
        catch (Exception ex) when (
            ex is MissingMethodException or MethodAccessException ||
            ex is InvalidCastException ||
            ex is ArgumentException)
        {
            Console.WriteLine($"Error creating instance of {className}: {ex.Message}");
            throw;
        }
    }

    public IAuthorizationService? CreateAuthenticationService(Func<string, bool> predicate, string className)
    {
        return CreateAuthorizationService(className);
    }

    /// <summary>
    ///  Returns a list of available Authorization service names.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<string> GetAvailableServiceNames()
    {
        return _authTypes.Keys;
    }

    
    /// <summary>
    ///  Checks if the provided class name is a valid Authorization service type.
    /// </summary>
    /// <param name="className">
    ///  The name of the class to be checked.
    /// </param>
    /// <returns></returns>
    public bool IsValidServiceType(string className)
    {
        return _authTypes.ContainsKey(className);
    }
    
    /// <summary>
    ///   Checks if the provided type is a valid Authorization service type.
    /// </summary>
    /// <param name="type">
    ///  The type to be checked.
    /// </param>
    /// <returns></returns>
    private static bool IsValidAuthorizationType(Type type)
    {
        if (!type.IsClass || type.IsAbstract)
            return false;

        if (type.GetConstructor(Type.EmptyTypes) == null)
            return false;

        // Check if the type implements IAuthorizationService
        return typeof(IAuthorizationService).IsAssignableFrom(type);
    }
}