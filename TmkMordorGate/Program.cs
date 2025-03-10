using TmkMordorGate.Config;


//Program.cs
var builder = WebApplication.CreateBuilder(args);
builder.ConfigureInitialServices();
var app = builder.Build();
app.ConfigureMiddlewares();

app.Run();