using Microsoft.EntityFrameworkCore;
using SupportSystem.User.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace SupportSystem.User.MSSQL.Backend.Deployment
{
    public class UserDatabase : DbContext
    {
        public UserDatabase() { }

        public UserDatabase(DbContextOptions options) : base(options)
        {
        }

        //public UserDatabase(DbContextOptions<UserDatabase> options) : base(options)
        //{


        //}

        public DbSet<PersonModel> SS_User { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Server=.;Database=SupportSystem;User Id=testnaren;Password=Naren@123;TrustServerCertificate=True;"
                //,sqlOptions => sqlOptions.MigrationsAssembly("SupportSystem.User.MSSQL.Backend")
                );
        }

        // Add the below code before you are insert value in to the table as per udemy video.
        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
          //  base.OnModelCreating(modelBuilder);
        //}

    }
}
