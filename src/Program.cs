using Blazored.SessionStorage;
using CurrieTechnologies.Razor.Clipboard;
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
builder.Services.AddMudExtensions();
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

builder.Services.Configure<List<Domain>>(builder.Configuration.GetSection("Domains"));
builder.Services.AddScoped<LdapService>();
builder.Services.AddScoped<SessionManagerService>();
builder.Services.AddSingleton<CryptService>();
builder.Services.AddOptions<LoginRateLimitOptions>()
    .Bind(builder.Configuration.GetSection("LoginRateLimit"))
    .Validate(o => !o.Enabled || (o.MaxFailedAttempts > 0 && o.WindowMinutes > 0), "LoginRateLimit: MaxFailedAttempts and WindowMinutes must be greater than 0")
    .ValidateOnStart();
builder.Services.AddSingleton<LoginThrottleService>();

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.Use(async (context, next) =>
{
    // MudBlazor sets inline styles, so style-src needs 'unsafe-inline'; scripts stay restricted to 'self'
    context.Response.Headers.ContentSecurityPolicy =
        "default-src 'self'; script-src 'self'; style-src 'self' 'unsafe-inline' https://fonts.googleapis.com; " +
        "font-src 'self' https://fonts.gstatic.com; img-src 'self' data:; object-src 'none'; base-uri 'self'; " +
        "form-action 'self'; frame-ancestors 'none'";
    context.Response.Headers.XContentTypeOptions = "nosniff";
    context.Response.Headers["Referrer-Policy"] = "no-referrer";
    await next();
});
app.UseStaticFiles();
app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");
app.MapHealthChecks("/healthz");

app.UseSerilogRequestLogging();
app.Run();
