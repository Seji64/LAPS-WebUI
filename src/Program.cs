using Blazored.SessionStorage;
using CurrieTechnologies.Razor.Clipboard;
using LAPS_WebUI.Interfaces;
using LAPS_WebUI.LogEnrichers;
using LAPS_WebUI.Models;
using LAPS_WebUI.Services;
using MudBlazor;
using MudBlazor.Services;
using MudExtensions.Services;
using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .CreateBootstrapLogger();

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSerilog((services, lc) => lc
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.With<AuditPrefixEnricher>()
    .Enrich.FromLogContext()
    .ReadFrom.Services(services));

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddMudServices(config =>
{
    config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomCenter;
    config.SnackbarConfiguration.ShowCloseIcon = true;
    config.SnackbarConfiguration.NewestOnTop = true;
    config.SnackbarConfiguration.SnackbarVariant = Variant.Filled;
});
builder.Services.AddMudExtensions();
builder.Services.AddBlazoredSessionStorage();
builder.Services.AddClipboard();
builder.Services.AddDataProtection();
builder.Services.AddHealthChecks();

builder.Services.Configure<List<Domain>>(builder.Configuration.GetSection("Domains"));
builder.Services.AddScoped<ILdapService, LdapService>();
builder.Services.AddScoped<ISessionManagerService, SessionManagerService>();
builder.Services.AddSingleton<ICryptService, CryptService>();

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");
app.MapHealthChecks("/healthz");

app.UseSerilogRequestLogging();
app.Run();
