using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using FlixFlow.Data;
using FlixFlow.Models;
using FlixFlow.Interfaces;
using FlixFlow.Services;

var builder = WebApplication.CreateBuilder(args);

// ===============================================
// DATABASE CONFIGURATION - SWITCHED TO SQLite
// ===============================================
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// ===============================================
// IDENTITY CONFIGURATION
// ===============================================
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Configure cookie settings
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.SlidingExpiration = true;
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
});

// ===============================================
// REGISTER SERVICES
// ===============================================
builder.Services.AddScoped<ITMDBService, TMDBService>();
builder.Services.AddScoped<IWatchlistService, WatchlistService>();
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<IFavoriteService, FavoriteService>();
builder.Services.AddScoped<IRatingService, RatingService>();

builder.Services.AddHttpClient<ITMDBService, TMDBService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["TMDB:BaseUrl"] ?? "https://api.themoviedb.org/3/");
});

// Configure TMDB options
builder.Services.Configure<TMDBOptions>(builder.Configuration.GetSection("TMDB"));

// Add MVC
builder.Services.AddControllersWithViews();

var app = builder.Build();

// ===============================================
// SEED DATABASE WITH ERROR HANDLING
// ===============================================
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var tmdbService = scope.ServiceProvider.GetRequiredService<ITMDBService>();

    // Ensure database is created
    dbContext.Database.EnsureCreated();

    try
    {
        await DbInitializer.Initialize(dbContext, tmdbService);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error seeding database: {ex.Message}");
        // Continue running - user can search movies later
    }
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

app.Run();

// ===============================================
// DB INITIALIZER CLASS
// ===============================================
public static class DbInitializer
{
    public static async Task Initialize(ApplicationDbContext context, ITMDBService tmdbService)
    {
        // Check if movies already exist
        if (context.Movies.Any()) return;

        try
        {
            var popularMovies = await tmdbService.GetPopularMoviesAsync();
            if (popularMovies.Any())
            {
                await context.Movies.AddRangeAsync(popularMovies);
                await context.SaveChangesAsync();
                Console.WriteLine($"✅ Seeded {popularMovies.Count} movies to database");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Could not seed movies: {ex.Message}");
            // Don't throw - app will still work
        }
    }
}