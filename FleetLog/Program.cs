namespace FleetLog;
using FleetLog.Data;
using Microsoft.EntityFrameworkCore;
using FleetLog.Services;
using Microsoft.AspNetCore.Identity;

public class Program
{
    public static async Task Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
        builder.Services.AddControllersWithViews();
        builder.Services.AddScoped<FuelEntryService>();
        builder.Services.AddScoped<RepairService>();
        builder.Services.AddScoped<VehicleService>();


        builder.Services.AddDbContext<FleetLogDbContext>(options =>
            options.UseSqlServer(
                builder.Configuration.GetConnectionString("DefaultConnection")));

        builder.Services
            .AddIdentity<IdentityUser, IdentityRole>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequiredLength = 8;
            })
            .AddEntityFrameworkStores<FleetLogDbContext>()
            .AddErrorDescriber<BulgarianIdentityErrorDescriber>()
            .AddDefaultTokenProviders();

        builder.Services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = "/Account/Login";
            options.AccessDeniedPath = "/Account/AccessDenied";
        });

        WebApplication app = builder.Build();

        await IdentitySeeder.SeedRolesAsync(app.Services);

        await DevelopmentAdminSeeder.SeedAsync(
            app.Services,
            app.Configuration,
            app.Environment);

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

        await app.RunAsync();
    }
}