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

app.UseHttpLogging();
app.UseExceptionHandler();
app.UseCors();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    await app.ApplyMigrations();
}

app.UseHttpsRedirection();
app.UseRateLimiter();
app.AddEndpoints();

app.Run();
