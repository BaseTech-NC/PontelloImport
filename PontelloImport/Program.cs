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

builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.LogoutPath = "/Identity/Account/Logout";
    options.AccessDeniedPath = "/Home/AccessDenied";
});
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
        PontelloDbInitializer.Seed(app);
        await SeedAuthAsync(userManager, roleManager, db);
    }
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "Database migration or seeding failed.");
    throw;
}

app.Run();

static async Task SeedAuthAsync(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager,
    PontelloDbContext db)
{
    // Seed roles
    string[] roles = { "SuperAdmin", "Admin", "Dealer" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }

    // Helper: create/update user + ensure correct role
    async Task<ApplicationUser?> EnsureUser(
        string email, string firstName, string lastName,
        string userType, string password, string[] assignedRoles)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
        {
            user = new ApplicationUser
            {
                UserName = email, Email = email,
                FirstName = firstName, LastName = lastName,
                UserType = userType, EmailConfirmed = true
            };
            var result = await userManager.CreateAsync(user, password);
            if (!result.Succeeded) return null;
        }
        else
        {
            // Always reset password so seed values are guaranteed correct on every startup
            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            await userManager.ResetPasswordAsync(user, token, password);
        }
        foreach (var role in assignedRoles)
        {
            if (!await userManager.IsInRoleAsync(user, role))
                await userManager.AddToRoleAsync(user, role);
        }
        return user;
    }

    // Admins
    await EnsureUser("jesse@pontelloimports.com", "Jesse", "Pontello",
        "SuperAdmin", "Admin@Pontello2026!", new[] { "SuperAdmin", "Admin" });
    await EnsureUser("kelly@pontelloimports.com", "Kelly", "Vlaar",
        "Admin", "Admin@Pontello2026!", new[] { "Admin" });

    // Helper: ensure Dealer record exists for an ApplicationUser
    async Task EnsureDealerRecord(
        string email, string companyName, string phone,
        string street, string city, string province, string postalCode)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user == null) return;

        // Skip if dealer record already exists
        if (db.Dealers.Any(d => d.ApplicationUserID == user.Id)) return;

        // Need a default PaymentTerms (Net 30)
        var terms = db.PaymentTerms.FirstOrDefault(t => t.TermCode == "NET30")
                    ?? db.PaymentTerms.FirstOrDefault();
        if (terms == null) return; // PaymentTerms not yet seeded — skip

        var billing = new Address
        {
            Street = street, City = city,
            Province = province, PostalCode = postalCode,
            Country = "Canada"
        };
        db.Addresses.Add(billing);
        await db.SaveChangesAsync();

        db.Dealers.Add(new Dealer
        {
            ApplicationUserID = user.Id,
            CompanyName = companyName,
            ContactPhone = phone,
            BillingAddressID = billing.AddressID,
            PaymentTermsID = terms.PaymentTermsID,
            IsTaxExempt = false
        });
        await db.SaveChangesAsync();
    }

    // 5 dealer accounts
    await EnsureUser("dealer1@testdealer.com", "Roman", "Velez",
        "Dealer", "Dealer@Test2026!", new[] { "Dealer" });
    await EnsureDealerRecord("dealer1@testdealer.com",
        "Roman Go-Karts", "416-555-0101",
        "100 Racing Blvd", "Toronto", "ON", "M5V 1A1");

    await EnsureUser("dealer2@testdealer.com", "Sandra", "Apex",
        "Dealer", "Dealer@Test2026!", new[] { "Dealer" });
    await EnsureDealerRecord("dealer2@testdealer.com",
        "Apex Racing Supply", "604-555-0202",
        "200 Speedway Dr", "Vancouver", "BC", "V6B 2W1");

    await EnsureUser("dealer3@testdealer.com", "Mike", "Zucker",
        "Dealer", "Dealer@Test2026!", new[] { "Dealer" });
    await EnsureDealerRecord("dealer3@testdealer.com",
        "SpeedZone Karting", "514-555-0303",
        "300 Circuit Ave", "Montreal", "QC", "H2X 1Y2");

    await EnsureUser("dealer4@testdealer.com", "Laura", "Track",
        "Dealer", "Dealer@Test2026!", new[] { "Dealer" });
    await EnsureDealerRecord("dealer4@testdealer.com",
        "Track Masters Inc", "403-555-0404",
        "400 Podium Crt", "Calgary", "AB", "T2P 3N5");

    await EnsureUser("dealer5@testdealer.com", "James", "Elite",
        "Dealer", "Dealer@Test2026!", new[] { "Dealer" });
    await EnsureDealerRecord("dealer5@testdealer.com",
        "Elite Kart Shop", "613-555-0505",
        "500 Victory Lane", "Ottawa", "ON", "K1A 0A9");
}
