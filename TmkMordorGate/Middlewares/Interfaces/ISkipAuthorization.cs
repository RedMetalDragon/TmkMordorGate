namespace TmkMordorGate.Middlewares.Interfaces;

public interface ISkipAuthorization: IMiddleware
{
    public Task SkipInvoke(HttpContext context, IEnumerable<string> pathToSkip);
}