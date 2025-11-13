using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Repository;


public static class DependencyInjectionSetup
{
    public static IServiceCollection AddRepositoryServices(this IServiceCollection services, string connectionString)
    {

        void DbContextOptions(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
            .UseSqlServer(connectionString, sqlOptions => { sqlOptions.CommandTimeout(30).EnableRetryOnFailure(); });
        }

        services.AddScoped<IIncrementRepository, IncrementRepository>();    // Register this relationship here because IncrementRepository is internal
        services.AddDbContext<IncrementContext>(DbContextOptions);   // Register the DbContext here because IncrementContext is internal
        return services;
    }
}
