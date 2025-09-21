using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using VehicleRegisterSystem.Application.Interfaces;
using VehicleRegisterSystem.Application.Services;
using VehicleRegisterSystem.Domain;
using VehicleRegisterSystem.Infrastructure.Data;
using VehicleRegisterSystem.Infrastructure.Identity;
using VehicleRegisterSystem.Infrastructure.Repositories;
using VehicleRegisterSystem.Web.GlobalExceptionFiltersl;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddScoped<IUserRepository, UserRepository>();   // Your implementation
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();   // Your implementation
builder.Services.AddScoped<IJwtService, JwtService>();           // Your implementation
builder.Services.AddScoped<IOrderRepository, OrderRepository>();           // Your implementation
builder.Services.AddScoped<IOrderService, OrderService>();           // Your implementation
// ILogger<T> is automatically provided by ASP.NET Core



builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<GlobalExceptionFilter>();
});



builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireDigit = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddErrorDescriber<ArabicIdentityErrorDescriber>() //  ”ÃÌ· «·›∆… »«·⁄—»Ì
.AddDefaultTokenProviders();





builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
});

// ≈÷«›… Œœ„«  ﬁ«⁄œ… «·»Ì«‰« 
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=(localdb)\\mssqllocaldb;Database=VehicleRegisterSystemDb;Trusted_Connection=true;MultipleActiveResultSets=true";

// ≈⁄œ«œ«  JWT
var jwtSection = builder.Configuration.GetSection("JwtSettings");
builder.Services.Configure<JwtSettings>(jwtSection);
var jwtSettings = jwtSection.Get<JwtSettings>();
// ≈÷«›… Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var token = context.Request.Cookies["AuthToken"];
            if (!string.IsNullOrEmpty(token))
                context.Token = token;
            return Task.CompletedTask;
        }
    };
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidateAudience = true,
        ValidAudience = jwtSettings.Audience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

builder.Services.AddControllers();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
// Seed roles and users
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    var context = services.GetRequiredService<AppDbContext>(); // Replace YourDbContext with your actual DbContext type

    try
    {
        // Wait for database to be available with retry logic
        var maxRetries = 10;
        var retryDelay = TimeSpan.FromSeconds(2);

        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                logger.LogInformation($"Attempting to connect to database (attempt {i + 1}/{maxRetries})...");

                if (await context.Database.CanConnectAsync())
                {
                    logger.LogInformation("Database connection successful!");
                    break;
                }
            }
            catch (Exception ex) when (i < maxRetries - 1)
            {
                logger.LogWarning($"Database not ready yet. Retrying in {retryDelay.TotalSeconds} seconds...");
                await Task.Delay(retryDelay);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to connect to database after all retry attempts");
                throw;
            }
        }
        // Apply migrations
        await context.Database.MigrateAsync();
        // Now seed the data
        await DbInitializer.SeedDataAsync(services);
        logger.LogInformation("Database seeding completed successfully!");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while seeding the database.");
        throw; // Re-throw to ensure the app doesn't start with a faulty database
    }
}
// ≈÷«›… «·‹ Middleware ›Ì √⁄·Ï «·‹ pipeline
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();

app.UseAuthentication(); // Add this

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}")
    .WithStaticAssets();


app.Run();
