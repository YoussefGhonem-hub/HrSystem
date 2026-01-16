using HrSystem.Infrustructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HrSystem.Infrustructure;

public static class ServicesRegistrationExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");


        services.AddDbContext<ApplicationDbContext>((opts) =>
        {
            opts.UseSqlServer(connectionString);
        });


        return services;
    }
}