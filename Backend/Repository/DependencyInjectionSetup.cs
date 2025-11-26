using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Repository;


public static class DependencyInjectionSetup
{
    // Register / Setup anything that is internal to the Repository project here
    public static IServiceCollection AddRepositoryServices(this IServiceCollection services, string connectionString)
    {
        void DbContextOptions(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
            .UseSqlServer(connectionString, sqlOptions => { sqlOptions.CommandTimeout(30).EnableRetryOnFailure(); });
        }

        services.AddScoped<IIncrementRepository, IncrementRepository>();
        services.AddDbContext<IncrementContext>(DbContextOptions);
        return services;
    }
}
