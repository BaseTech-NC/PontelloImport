using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PontelloImport.Data;
using PontelloImport.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Register HttpContextAccessor for audit tracking
builder.Services.AddHttpContextAccessor();

builder.Services.AddDbContext<PontelloDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
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
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PontelloDbContext>();
    db.Database.Migrate();
    var appDb = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    appDb.Database.Migrate();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    await SeedAuthAsync(userManager, roleManager);
}
PontelloDbInitializer.Seed(app);

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
