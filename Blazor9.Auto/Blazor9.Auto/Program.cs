using Blazor9.Auto.Client.Services;
using Blazor9.Auto.Components;
using Blazor9.Auto.Components.Account;
using Blazor9.Auto.Data;
using Blazor9.Auto.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents()
    .AddAuthenticationStateSerialization();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, PersistingRevalidatingAuthenticationStateProvider>();
//builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddIdentityCookies();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options => {
    options.UseSqlServer(connectionString);
    options.EnableSensitiveDataLogging(true);
});
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddRoleManager<RoleManager<IdentityRole>>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.Configure<IdentityOptions>(options => {
    options.Password.RequiredLength = 1;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireDigit = false;

    options.User.RequireUniqueEmail = true;
    //options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";

    //options.SignIn.RequireConfirmedPhoneNumber = false;
    //options.SignIn.RequireConfirmedEmail = false;

    options.Lockout.AllowedForNewUsers = false;
    //options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    //options.Lockout.MaxFailedAccessAttempts = 5;
});

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

// register server-based implementation to integrate with an API
builder.Services.AddScoped<IApiService, ServerApiService>();

builder.Services.AddHttpClient("LocalAPIClient", cfg =>
{
    cfg.BaseAddress = new Uri(
        builder.Configuration["LocalAPIBaseAddress"] ??
            throw new Exception("LocalAPIBaseAddress is missing."));
});
builder.Services.AddHttpClient("RemoteAPIClient", cfg =>
{
    cfg.BaseAddress = new Uri(
       builder.Configuration["RemoteAPIBaseAddress"] ??
           throw new Exception("RemoteAPIBaseAddress is missing."));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.MapControllers();
app.MapControllerRoute("controllers",
 "controllers/{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(Blazor9.Auto.Client._Imports).Assembly);

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

var context = app.Services.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();
SeedData.EnsurePopulated(context);
IdentitySeedData.EnsurePopulated(app.Services, app.Configuration);

// define a local API for testing
app.MapGet("localapi/bands", () =>
{
    return Results.Ok(new[] { new { Id = 1, Name = "Nirvana (from local API)" },
        new { Id = 2, Name = "Queens of the Stone Age (from local API)" },
        new { Id = 3, Name = "Fred Again. (from local API)" },
        new { Id = 4, Name = "Underworld (from local API)" } });
});

app.Run();
