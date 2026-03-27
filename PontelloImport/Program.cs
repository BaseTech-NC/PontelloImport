using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PontelloImport.Data;
using PontelloImport.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Use absolute paths so SQLite files land in the app root on both local and Azure (D:\home\site\wwwroot)
var contentRoot = builder.Environment.ContentRootPath;
var connectionString = $"Data Source={Path.Combine(contentRoot, "ApplicationDatabase.db")}";
var pontelloConnectionString = $"Data Source={Path.Combine(contentRoot, "PontelloImportDatabase.db")}";

// Register HttpContextAccessor for audit tracking
builder.Services.AddHttpContextAccessor();

builder.Services.AddDbContext<PontelloDbContext>(options =>
    options.UseSqlite(pontelloConnectionString));

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

<<<<<<< HEAD
builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
=======
builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
>>>>>>> d51b3375d7885e96df984aa979bd5083c4de4923
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();

// Session-based TempData for Review-before-Save flow (cookie TempData has ~4KB limit)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
   .WithStaticAssets();


// Apply EF migrations, then seed reference/test data
try
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<PontelloDbContext>();
        db.Database.Migrate();
        var appDb = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        appDb.Database.Migrate();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        await SeedAuthAsync(userManager, roleManager);
        PontelloDbInitializer.Seed(app);
    }
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "Database migration or seeding failed.");
    throw;
}

app.Run();

static async Task SeedAuthAsync(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
{
    // Seed roles
    string[] roles = { "Admin", "Dealer" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }
    // Seed Jesse — SuperAdmin
    if (await userManager.FindByEmailAsync("jesse@pontelloimports.com") == null)
    {
        var jesse = new ApplicationUser
        {
            UserName = "jesse@pontelloimports.com",
            Email = "jesse@pontelloimports.com",
            FirstName = "Jesse",
            LastName = "Pontello",
            UserType = "SuperAdmin",
            EmailConfirmed = true
        };
        await userManager.CreateAsync(jesse, "Admin@123");
        await userManager.AddToRoleAsync(jesse, "Admin");
    }
    // Seed Kelly — Admin
    if (await userManager.FindByEmailAsync("kelly@pontelloimports.com") == null)
    {
        var kelly = new ApplicationUser
        {
            UserName = "kelly@pontelloimports.com",
            Email = "kelly@pontelloimports.com",
            FirstName = "Kelly",
            LastName = "Vlaar",
            UserType = "Admin",
            EmailConfirmed = true
        };
        await userManager.CreateAsync(kelly, "Admin@123");
        await userManager.AddToRoleAsync(kelly, "Admin");
    }
    // Seed test dealer
    if (await userManager.FindByEmailAsync("dealer@testdealer.com") == null)
    {
        var dealer = new ApplicationUser
        {
            UserName = "dealer@testdealer.com",
            Email = "dealer@testdealer.com",
            FirstName = "Test",
            LastName = "Dealer",
            UserType = "Dealer",
            EmailConfirmed = true
        };
        await userManager.CreateAsync(dealer, "Dealer@123");
        await userManager.AddToRoleAsync(dealer, "Dealer");
    }
}
