namespace TmkMordorGate.Middlewares.Interfaces;

public interface IMiddleware
{
    RequestDelegate? Next { get; set; }
    
    Task Invoke(HttpContext context);
}