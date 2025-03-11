using System.Collections.Immutable;
using System.Reflection;
using System.Globalization;
using TmkMordorGate.Config.Interfaces;
using TmkMordorGate.Services.Authorization;
using TmkMordorGate.Services.Interfaces;
using Yarp.ReverseProxy.Configuration;

namespace TmkMordorGate.Helpers;

public sealed class AuthorizationFactory : IAuthorizationFactory
{
    private readonly ImmutableDictionary<string, Type> _authTypes;
    private readonly IConfiguration _configuration;
    private IServiceProvider? _serviceProvider;
    private IDictionary<string, IAuthorizationService>? _authorizationServices;

    public AuthorizationFactory()
    {
        _serviceProvider = null;
        _authTypes = FindAuthorizationServiceTypes(CultureInfo.CurrentCulture);
    }

    public AuthorizationFactory(IServiceProvider? serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _authTypes = FindAuthorizationServiceTypes(CultureInfo.CurrentCulture);
        _configuration = serviceProvider.GetRequiredService<IConfiguration>();
        _authorizationServices = new Dictionary<string, IAuthorizationService>();
    }

    private static ImmutableDictionary<string, Type> FindAuthorizationServiceTypes(CultureInfo cultureInfo,
        bool ignoreCase = true)
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

    public IAuthorizationService? CreateAuthorizationInstance(string className)
    {
        try
        {
            if (!_authTypes.TryGetValue(className, out var type))
            {
                return null;
            }

            // Create an instance of the type using the service provider if available
            if (_serviceProvider != null)
                return (IAuthorizationService)ActivatorUtilities.CreateInstance(_serviceProvider, type)!;
            // Otherwise, create an instance of the type using the default constructor
            var instance = (IAuthorizationService)Activator.CreateInstance(type)!;
            // add try cath for already existing key
            try
            {
                _authorizationServices?.Add(className, instance);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            return instance;
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

    public IAuthorizationService? CreateAuthorizationInstance(Func<string, bool> predicate, string className)
    {
        return CreateAuthorizationInstance(className);
    }

    /// <summary>
    /// Returns a new instance of an IAuthorizationService based on the provided class name, predicate, and target route.
    /// </summary>
    /// <param name="predicate">
    ///   The predicate function to determine if the class name matches the desired criteria.
    /// </param>
    /// <param name="className">
    ///  The name of the class to be used for authorization.
    /// </param>
    /// <param name="targetRoute">
    /// The target route for the authorization service.
    /// </param>
    /// <returns>
    ///  Returns a new instance of an IAuthorizationService based on the provided class name, predicate, and target route.
    ///  But as side effect the instance is added to the _authorizationServices dictionary.
    /// </returns>
    public IAuthorizationService? CreateAuthorizationInstance(Func<string, bool> predicate, string className,
        string targetRoute)
    {
        if (!predicate(className))
            throw new ArgumentException("Predicate function not passed", nameof(className));
        var instance = CreateAuthorizationInstance(className);
        if (instance != null)
            _authorizationServices?.Add(targetRoute, instance);
        return instance;
    }

    public IAuthorizationService? GetAuthorizationService(HttpContext context)
    {
        // Based on the request path, get the IAuthorizationService instance
        var key = GetKeyByPath(context);
        if (string.IsNullOrEmpty(key))
            return new Dummy();
        if (_authorizationServices?.TryGetValue(key, out var service) ?? false)
            return service;
        return new Dummy();
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

        //if (type.GetConstructor(Type.EmptyTypes) == null)
        //    return false;

        // Check if the type implements IAuthorizationService
        return typeof(IAuthorizationService).IsAssignableFrom(type);
    }

    /// <summary>
    ///  Returns the route name based on the provided path.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    private string? GetRouteName(string path)
    {
        var yarpRoutes = _configuration.GetSection("ReverseProxy:Routes").Get<Dictionary<string, RouteConfig>>();
        return yarpRoutes?.FirstOrDefault(route => path.StartsWith(route.Value.Match.Path ?? string.Empty)).Key;
    }

    /// <summary>
    ///  Returns the route name based on the provided HttpContext.
    /// </summary>
    /// <param name="context">
    /// The HttpContext object.
    /// </param>
    /// <returns></returns>
    private string? GetKeyByPath(HttpContext context)
    {
        if (!context.Request.Path.HasValue)
            return string.Empty;
        var path = context.Request.Path.Value;
        if (!path.Contains("/api/v"))
            return string.Empty;
        var routeMappings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "brain", "routeCore" },
            { "gondor", "routeGondor" },
            { "tools", "routeTools" }
            //{ "gondor", "routeGondor" }
            // New routes can be easily added here
        };

        return routeMappings.FirstOrDefault(mapping => path.Contains(mapping.Key)).Value;
    }
}