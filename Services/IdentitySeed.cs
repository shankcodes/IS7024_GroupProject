using Microsoft.AspNetCore.Identity;

namespace IS7024_GroupProject.Services;

public static class IdentitySeed
{
    public const string AdminRoleName = "Admin";

    public static async Task SeedRolesAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole<int>>>();

        if (!await roleManager.RoleExistsAsync(AdminRoleName))
        {
            var result = await roleManager.CreateAsync(new IdentityRole<int>(AdminRoleName));

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(error => error.Description));
                throw new InvalidOperationException($"Unable to seed the {AdminRoleName} role: {errors}");
            }
        }
    }
}