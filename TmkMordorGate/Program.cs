using TmkMordorGate.Config;
using TmkMordorGate.Config.Interfaces;

var builder = WebApplication.CreateBuilder(args);
builder.ConfigureInitialServices();
var app = builder.Build();
app.TmkConfigureMiddleWares();

// Instantiate the authorization factory
// and create authorization instances
var authorizationFactory = app.Services.GetRequiredService<IAuthorizationFactory>();
var authorizationInstances = app.Configuration.GetSection("AuthorizationInstances").GetChildren();
foreach (var instance in authorizationInstances)
{
    var className = instance.GetValue<string>("Name");
    var targetRoute = instance.GetValue<string>("Route");

    if (string.IsNullOrEmpty(className))
        throw new ArgumentException("Class name cannot be null or empty", nameof(className));

    if (string.IsNullOrEmpty(targetRoute))
        throw new ArgumentException("Target route cannot be null or empty", nameof(targetRoute));

    // Use a valid predicate to filter authorization types if needed
    authorizationFactory.CreateAuthorizationInstance(s => s.Length > 0, className, targetRoute);
}

// Run the application
app.Run();