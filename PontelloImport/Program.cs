using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PontelloImport.Data;
using PontelloImport.Models;
using PontelloImport.Services;
using QuestPDF.Infrastructure;

QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);
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
>>>>>>> d51b3375d7885e96df984aa979bd5083c4de4923
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

// Email service
builder.Services.AddScoped<IEmailService, EmailService>();

// PDF service
builder.Services.AddScoped<IPdfService, PdfService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseSession();

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
    var appDb = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    try
    {
        db.Database.Migrate();
        appDb.Database.Migrate();
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine("MIGRATION FAILED: " + ex.Message);
        Console.Error.WriteLine(ex.StackTrace);
        throw;
    }

    try
    {
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        await PontelloDbInitializer.Seed(app);
        await SeedAuthAsync(userManager, roleManager, db);
    }
    catch (Exception ex)
    {
        Console.WriteLine("=== SEED ERROR ===");
        Console.WriteLine(ex.ToString());
        Console.WriteLine("=== END ERROR ===");
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Seeding failed.");
        throw;
    }
}

app.Run();

static async Task SeedAuthAsync(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager,
    PontelloDbContext db)
{
    // Seed roles
    string[] roles = { "SuperAdmin", "Admin", "Dealer", "Staff" };
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

    // SuperAdmins
    await EnsureUser("jesse@pontelloimports.com", "Jesse", "Pontello",
        "SuperAdmin", "Admin@Pontello2026!", new[] { "SuperAdmin", "Admin" });
    await EnsureUser("kelly@pontelloimports.com", "Kelly", "Vlaar",
        "Admin", "Admin@Pontello2026!", new[] { "Admin" });

    // Admin portal account
    await EnsureUser("admin@pontelloimports.com", "Portal", "Admin",
        "Admin", "Admin@Portal2026!", new[] { "Admin" });

    // Staff accounts
    await EnsureUser("staff1@pontelloimports.com", "Staff", "One",
        "Staff", "Staff@Portal2026!", new[] { "Staff" });
    await EnsureUser("staff2@pontelloimports.com", "Staff", "Two",
        "Staff", "Staff@Portal2026!", new[] { "Staff" });

    // Helper: ensure Dealer record exists for an ApplicationUser
    async Task EnsureDealerRecord(
        string email, string companyName, string phone,
        string street, string city, string province, string postalCode)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user == null) return;

        if (db.Dealers.Any(d => d.ApplicationUserID == user.Id)) return;

        var terms = db.PaymentTerms.FirstOrDefault(t => t.TermCode == "NET30")
                    ?? db.PaymentTerms.FirstOrDefault();
        if (terms == null) return;

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
