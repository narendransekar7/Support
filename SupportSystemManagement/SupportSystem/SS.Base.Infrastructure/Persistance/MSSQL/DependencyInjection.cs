using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SS.Base.Domain.Interfaces.Repository;
using SS.Base.Infrastructure.Persistance.MSSQL.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SS.Base.Infrastructure.Persistance.MSSQL
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<MSSQLDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            // Add other infrastructure services (e.g., repositories, external integrations)

            // Register Unit of Work for adding DB save logic in Infrastructure layer instead of Application layer.
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Register Repositories MSSQLDbContext _context need to be in infrastructure layer instead of Application layer
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ITicketRepository, TicketRepository>();
            services.AddScoped<ITicketUpdateRepository, TicketUpdateRepository>();

            return services;
        }
    }
}
