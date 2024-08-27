namespace TmkMordorGate.Middlewares;

public interface ISetHeader : IMiddleware
{
     Task SetHeader(HttpContext context,string key, string value);
}