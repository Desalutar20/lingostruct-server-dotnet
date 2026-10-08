using src.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseDefaultServiceProvider(config =>
{
    config.ValidateOnBuild = true;
    config.ValidateScopes = true;
});
builder.WebHost.UseKestrel(options => options.AddServerHeader = false);

builder.Services.AddOpenApi();
builder.Services.ConfigureServices(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    await app.ApplyMigrations();
}

app.UseHttpsRedirection();
app.AddEndpoints();

app.Run();
