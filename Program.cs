using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.FluentUI.AspNetCore.Components;
using ReachingOutDB.Components;
using ReachingOutDB.Data;
using Syncfusion.Blazor;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddFluentUIComponents();

//Custom code
builder.Services.AddSyncfusionBlazor();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var isDevelopment = builder.Environment.IsDevelopment();

builder.Services.AddDbContextFactory<AppDbContext>(options =>
{
    if (isDevelopment)
    {
        options.UseNpgsql(connectionString);
    }
    else
    {
        options.UseNpgsql(connectionString);
    }
});



builder.Services.AddScoped<CustomerServices>();
builder.Services.AddScoped<OrderServices>();
builder.Services.AddScoped<OrderAuditLogServices>();
builder.Services.AddScoped<UserServices>();
builder.Services.AddScoped<PackageServices>();
builder.Services.AddScoped<ShippingSettingsServices>();
builder.Services.AddScoped<MiscSettingsServices>();
builder.Services.AddScoped<PlateServices>();

// Set default culture
var defaultCulture = new CultureInfo("en-US");
CultureInfo.DefaultThreadCurrentCulture = defaultCulture;
CultureInfo.DefaultThreadCurrentUICulture = defaultCulture;

var app = builder.Build();

Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Mzk2OTA1NEAzMzMwMmUzMDJlMzAzYjMzMzAzYlIvQ2R1NCtubEwwVGJtMHhQcUxlZ3RSa0dLYWZGdTk2RXRad2hjUE9IT1U9");

// Apply migrations automatically on startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        context.Database.Migrate();
    }
    catch (Exception ex)
    {
        // Log the error (you might want to add logging here)
        Console.WriteLine($"An error occurred while migrating the database: {ex.Message}");
        throw;
    }
}

// Configure localization
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("en-US"),
    SupportedCultures = new[] { new CultureInfo("en-US") },
    SupportedUICultures = new[] { new CultureInfo("en-US") }
});

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
