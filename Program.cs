
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QRCodeManagerRelease2.Data;
using QRCodeManagerRelease2.Models;
using QRCodeManagerRelease2.Services;

var builder = WebApplication.CreateBuilder(args);

// Configurazione database
var connectionString = "Server=SRV1SIG\\SQLEXPRESS;Database=QRCodeManagerDB_R2;User Id=DB3;Password=Sinergieit2025;TrustServerCertificate=true;";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Servizi per le sessioni
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddDatabaseDeveloperPageExceptionFilter();


builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

// Configurazione Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 4;
})
.AddEntityFrameworkStores<ApplicationDbContext>()

.AddDefaultTokenProviders();

builder.Services.AddScoped<IExportService, ExportService>();

// Registrazione servizi
builder.Services.AddScoped<IQRCodeService, QRCodeService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddHttpClient();

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

// Configure pipeline
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
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseSession();

// Route per s=[codice] con gestione migliorata
app.MapGet("/s={code}", async (string code, ApplicationDbContext db, IServiceProvider services, HttpContext httpContext) =>
{
    if (string.IsNullOrEmpty(code))
    {
        return Results.NotFound();
    }

    var qrCode = await db.QRCodes
        .Include(q => q.CreatedBy)
        .FirstOrDefaultAsync(q => q.ExtractedCode == code);

    if (qrCode == null)
    { 
        httpContext.Session.SetString("ErrorMessage", "Il codice sigillo non è stato ancora configurato");
       
        return Results.Redirect($"/QRCode/CodesiaNotFound?code={code}");

        // Codice non trovato - pagina di cortesia
        //return Results.Redirect("/QRCode/CodesiaNotFound/s={code}");
    }

    if (qrCode.Bloccato)
    {
        // Codice disabilitato
        httpContext.Session.SetString("ErrorMessage", "Il link è stato disabilitato");
        return Results.Redirect("/QRCode/CodeDisabled");
    }

    // Registra la chiamata
    var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString();
    var userAgent = httpContext.Request.Headers["User-Agent"].ToString();

    var callHistory = new QRCodeCallHistory
    {
        QRCodeId = qrCode.Id,
        DataChiamata = DateTime.Now,
        IPAddress = ipAddress,
        UserAgent = userAgent
    };

    db.QRCodeCallHistories.Add(callHistory);

    // Aggiorna contatore
    qrCode.NumeroChiamate++;
    qrCode.UltimaChiamata = DateTime.Now;

    // Notifica prima volta se richiesta
    if (qrCode.AvvisamiPrimaVolta && !qrCode.NotificatoPrimaVolta && qrCode.CreatedBy?.Email != null)
    {
        var emailService = services.GetRequiredService<IEmailService>();
        await emailService.SendQRCodeFirstAccessNotificationAsync(qrCode.CreatedBy.Email, code);
        qrCode.NotificatoPrimaVolta = true;
    }

    await db.SaveChangesAsync();

    httpContext.Session.SetString("CurrentQRCode", code);

    return Results.Redirect("/QRCode/Access");
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

// Seed utenti
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var dbContext = services.GetRequiredService<ApplicationDbContext>();

        SeedData.Initialize(userManager, roleManager, dbContext).Wait();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Errore durante il seeding del database.");
    }
}

app.Run();
