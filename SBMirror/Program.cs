using Polly.Extensions.Http;
using Polly;
using SBMirror.Components;
using SBMirror.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

var retryPolicy = HttpPolicyExtensions.HandleTransientHttpError()
    .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

builder.Services.AddHttpClient("www", client =>
{
    client.DefaultRequestHeaders.UserAgent.ParseAdd("myAPI");
}).AddPolicyHandler(retryPolicy);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddSingleton<AmbientWeatherService>();
builder.Services.AddSingleton<NationalWeatherService>();
builder.Services.AddSingleton<CountdownService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(SBMirror.Client._Imports).Assembly);

app.Run();
