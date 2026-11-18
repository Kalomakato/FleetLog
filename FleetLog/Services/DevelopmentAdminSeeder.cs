using Microsoft.AspNetCore.Identity;

namespace FleetLog.Services;

public static class DevelopmentAdminSeeder
{
    public static async Task SeedAsync(
        IServiceProvider serviceProvider,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        if (!environment.IsDevelopment())
        {
            return;
        }

        var email = configuration["DevelopmentAdmin:Email"]?.Trim();
        var password = configuration["DevelopmentAdmin:Password"];

        if (string.IsNullOrWhiteSpace(email) &&
            string.IsNullOrWhiteSpace(password))
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException(
                "Configure both DevelopmentAdmin:Email and DevelopmentAdmin:Password.");
        }

        using var scope = serviceProvider.CreateScope();

        var userManager = scope.ServiceProvider
            .GetRequiredService<UserManager<IdentityUser>>();

        var user = await userManager.FindByEmailAsync(email);

        if (user is null)
        {
            user = new IdentityUser
            {
                UserName = email,
                Email = email
            };

            var createResult = await userManager.CreateAsync(user, password);

            if (!createResult.Succeeded)
            {
                throw new InvalidOperationException(
                    "Could not create the development administrator: " +
                    string.Join("; ", createResult.Errors.Select(e => e.Code)));
            }
        }
        else if (!await userManager.CheckPasswordAsync(user, password))
        {
            throw new InvalidOperationException(
                "The configured development administrator password does not match the existing account.");
        }

        if (!await userManager.IsInRoleAsync(user, RoleNames.Administrator))
        {
            var roleResult = await userManager.AddToRoleAsync(
                user, RoleNames.Administrator);

            if (!roleResult.Succeeded)
            {
                throw new InvalidOperationException(
                    "Could not assign the Administrator role: " +
                    string.Join("; ", roleResult.Errors.Select(e => e.Code)));
            }
        }
    }
}