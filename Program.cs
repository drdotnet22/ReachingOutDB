using Microsoft.FluentUI.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using ReachingOutDB.Components;
using ReachingOutDB.Data;
using Syncfusion.Blazor;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddFluentUIComponents();

//Custom code
builder.Services.AddSyncfusionBlazor();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseSqlServer(connectionString).EnableSensitiveDataLogging());

builder.Services.AddScoped<CustomerServices>();
builder.Services.AddScoped<OrderServices>();
builder.Services.AddScoped<OrderAuditLogServices>();
builder.Services.AddScoped<UserServices>();
builder.Services.AddScoped<PackageServices>();
builder.Services.AddScoped<ShippingSettingsServices>();
builder.Services.AddScoped<MiscSettingsServices>();
builder.Services.AddScoped<PlateServices>();

var app = builder.Build();

Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("MzkwNzg2OEAzMjM5MmUzMDJlMzAzYjMyMzkzYmlPa2xZa2MwblpXeUg3VFN6M3BzczdhT1VXbVhrUldvYlNWa0VpOTM4clE9");


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
