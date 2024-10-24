namespace TmkMordorGate.Middlewares.Interfaces;

public interface ISkipAuthentication : IMiddleware
{
    public Task SkipInvoke(HttpContext context, IEnumerable<string> pathToSkip);
}