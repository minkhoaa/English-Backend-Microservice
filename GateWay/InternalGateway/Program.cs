var builder = WebApplication.CreateBuilder(args);




builder.Services.AddReverseProxy().LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));



var app = builder.Build();


app.MapGet("/healthz", () => "Ok!");
app.MapReverseProxy();
app.Run();
