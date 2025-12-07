var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.MapGet("/", () => "The Modern Stoic API is running!");

app.Run();
