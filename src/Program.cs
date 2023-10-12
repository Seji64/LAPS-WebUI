using Blazored.SessionStorage;
using CurrieTechnologies.Razor.Clipboard;
using LAPS_WebUI.Interfaces;
using LAPS_WebUI.Models;
using LAPS_WebUI.Services;
using MudBlazor;
using MudBlazor.Services;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, lc) => lc
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.Console());

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddMudServices(config =>
{
    config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomCenter;
    config.SnackbarConfiguration.ShowCloseIcon = true;
    config.SnackbarConfiguration.NewestOnTop = true;
    config.SnackbarConfiguration.SnackbarVariant = Variant.Filled;
});
builder.Services.AddBlazoredSessionStorage();
builder.Services.AddClipboard();
builder.Services.AddDataProtection();
builder.Services.AddHealthChecks();

builder.Services.Configure<LdapOptions>(builder.Configuration.GetSection("LDAP"));
builder.Services.Configure<LapsOptions>(builder.Configuration.GetSection("LAPS"));
builder.Services.AddScoped<ILdapService, LdapService>();
builder.Services.AddScoped<ISessionManagerService, SessionManagerService>();
builder.Services.AddSingleton<ICryptService, CryptService>();

var app = builder.Build();

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
