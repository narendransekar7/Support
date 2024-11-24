using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SS.Base.Infrastructure.Persistance.MSSQL
{
    public class MSSQLDbContextFactory : IDesignTimeDbContextFactory<MSSQLDbContext>
    {
        public MSSQLDbContext CreateDbContext(string[] args)
        {
         
            // Relative path to SS.Web.API
            string relativePath = @"..\SS.Web.API";

            // Combine base directory with relative path
            string fullPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), relativePath));

            // Build configuration from appsettings.json
            var configuration = new ConfigurationBuilder()
                .SetBasePath(fullPath) // Points to the project folder
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            // Create options builder
            var optionsBuilder = new DbContextOptionsBuilder<MSSQLDbContext>();
            optionsBuilder.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));

            return new MSSQLDbContext(optionsBuilder.Options);
        }
    }
}
