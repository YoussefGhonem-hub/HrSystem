using HrSystem.Domain.Entities.Account;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;

namespace HrSystem.Infrustructure.Persistence;

public static class AppDbContextSeed
{
    public static async Task SeedAsync(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        IWebHostEnvironment env)
    {

    }


}