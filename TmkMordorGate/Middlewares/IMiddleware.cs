namespace TmkMordorGate.Middlewares;

public interface IMiddleware
{
    RequestDelegate Next { get; set; }
    
    Task Invoke(HttpContext context);
}