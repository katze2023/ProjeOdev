using FitnessCenterManagement.Data;
using FitnessCenterManagement.Models;
using FitnessCenterManagement.Services; // IGeminiAIService ve GeminiAIService burada varsayılır
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Connection string
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// 🔑 IDENTITY – TEK VE DOĞRU TANIM
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();

// Login path
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

// =========================================================
// 🔄 AI SERVİS DÜZELTMELERİ
// 1. HttpClient eklenir (AI servisi HTTP çağrısı yapacağı için gereklidir)
builder.Services.AddHttpClient();

// 2. Arayüz (IGeminiAIService) ve Uygulama (GeminiAIService) kaydedilir.
// Controller'ınız bu arayüzü kullanır.
builder.Services.AddScoped<IGeminiAIService, GeminiAIService>();
// =========================================================


builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

// PIPELINE
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// AREA ROUTES
app.MapAreaControllerRoute(
    name: "admin",
    areaName: "Admin",
    pattern: "Admin/{controller=Home}/{action=Index}/{id?}");

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// MVC
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Identity Razor Pages
app.MapRazorPages();


using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var context = services.GetRequiredService<ApplicationDbContext>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

    // Roller + Admin
    await DbInitializer.SeedRolesAndAdmin(userManager, roleManager);

    // TÜM DEMO VERİLER
    await DbSeeder.SeedDemoData(context, userManager);
}

app.Run();