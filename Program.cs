using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using CargoParcelTracker.Data;
using CargoParcelTracker.Models;
using CargoParcelTracker.Repositories.Interfaces;
using CargoParcelTracker.Repositories.Implementations;
using CargoParcelTracker.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add SignalR for real-time updates
builder.Services.AddSignalR();

// Add API Controllers with JSON options
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null; // Use PascalCase
        options.JsonSerializerOptions.WriteIndented = true;
    });

// Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Cargo Parcel Tracker API",
        Version = "v1",
        Description = "RESTful API for managing crude oil tanker cargo parcels, vessels, and voyage allocations",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Maritime Logistics Inc",
            Email = "info@maritimelogistics.com"
        }
    });
});

// Configure Entity Framework Core with SQLite
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure ASP.NET Core Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Password settings (relaxed for demo purposes)
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;

    // User settings
    options.User.RequireUniqueEmail = true;

    // Sign-in settings
    options.SignIn.RequireConfirmedEmail = false;
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Configure application cookie
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(24);
    options.SlidingExpiration = true;
});

// Register Repository Pattern with Dependency Injection
// Generic Repository
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

// Specific Repositories
builder.Services.AddScoped<ICargoParcelRepository, CargoParcelRepository>();
builder.Services.AddScoped<IVesselRepository, VesselRepository>();
builder.Services.AddScoped<IVoyageAllocationRepository, VoyageAllocationRepository>();

var app = builder.Build();

// Seed database with sample data and identity
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        // Seed domain data
        await DbSeeder.SeedAsync(context);

        // Seed roles and users
        await IdentitySeeder.SeedRolesAndUsersAsync(userManager, roleManager);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Cargo Parcel Tracker API v1");
        c.RoutePrefix = "api/docs"; // Access Swagger UI at /api/docs
    });
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllers(); // Map API controllers

// Map SignalR hub
app.MapHub<ParcelStatusHub>("/hubs/parcelStatus");

app.Run();
