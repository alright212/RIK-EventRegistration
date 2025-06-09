using EventRegistration.Application;
using EventRegistration.Domain;
using EventRegistration.Infrastructure;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configure DbContext - Auto-detect database provider
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Railway might also provide DATABASE_URL environment variable
if (string.IsNullOrEmpty(connectionString))
{
    connectionString = builder.Configuration["DATABASE_URL"];
}

// Debug logging for connection string
Console.WriteLine($"Connection string: {connectionString ?? "NULL"}");
Console.WriteLine($"Environment: {builder.Environment.EnvironmentName}");

// Check all environment variables that start with CONNECTION or DATABASE
Console.WriteLine("All connection-related environment variables:");
foreach (
    var envVar in Environment.GetEnvironmentVariables().Cast<System.Collections.DictionaryEntry>()
)
{
    var key = envVar.Key?.ToString() ?? "";
    if (key.ToUpper().Contains("CONNECTION") || key.ToUpper().Contains("DATABASE"))
    {
        Console.WriteLine($"  {key}: {envVar.Value}");
    }
}

// Ensure we have a valid connection string
if (string.IsNullOrEmpty(connectionString))
{
    connectionString = "DataSource=eventregistration.db"; // Fallback to SQLite
    Console.WriteLine("Using fallback SQLite connection string");
}

if (
    !string.IsNullOrEmpty(connectionString)
    && (
        connectionString.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase)
        || connectionString.Contains("postgres", StringComparison.OrdinalIgnoreCase)
    )
)
{
    // Use PostgreSQL for production (Railway, etc.)
    Console.WriteLine("Using PostgreSQL provider");

    // Convert Railway PostgreSQL URL to proper connection string format
    string pgConnectionString = connectionString;
    if (connectionString.StartsWith("postgresql://"))
    {
        var uri = new Uri(connectionString);
        pgConnectionString =
            $"Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.Trim('/')};Username={uri.UserInfo.Split(':')[0]};Password={uri.UserInfo.Split(':')[1]};SSL Mode=Require;Trust Server Certificate=true;";
        Console.WriteLine(
            $"Converted connection string: {pgConnectionString.Replace(uri.UserInfo.Split(':')[1], "****")}"
        );
    }

    builder.Services.AddDbContext<EventRegistrationDbContext>(options =>
        options.UseNpgsql(pgConnectionString)
    );
}
else
{
    // Use SQLite for development
    Console.WriteLine("Using SQLite provider");
    builder.Services.AddDbContext<EventRegistrationDbContext>(options =>
        options.UseSqlite(connectionString)
    );
}

// Register your repositories and services
// The framework will now 'inject' these wherever they are requested
builder.Services.AddScoped<IEventRepository, EventRepository>();
builder.Services.AddScoped<IParticipantRepository, ParticipantRepository>();
builder.Services.AddScoped<IPaymentMethodRepository, PaymentMethodRepository>();
builder.Services.AddScoped<IEventParticipantRepository, EventParticipantRepository>();

builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<IParticipantService, ParticipantService>();

// --- Add services required for TempData to work with redirects ---
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(20); // Set a session timeout
    options.Cookie.HttpOnly = true;
    // Make the session cookie essential to bypass cookie policy checks,
    // which is a common cause for TempData to fail.
    options.Cookie.IsEssential = true;
});

// ----------------------------------------------------------------

var app = builder.Build();

// Ensure database is created and migrations are applied
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<EventRegistrationDbContext>();
    context.Database.EnsureCreated();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// --- Add the session middleware to the request pipeline ---
// This must be called after UseRouting() and before MapControllerRoute().
app.UseSession();

// --------------------------------------------------------

app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
