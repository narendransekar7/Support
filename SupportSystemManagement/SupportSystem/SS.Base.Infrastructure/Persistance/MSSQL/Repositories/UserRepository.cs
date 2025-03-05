using Microsoft.EntityFrameworkCore;
using SS.Base.Domain.Entities;
using SS.Base.Domain.Interfaces.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SS.Base.Infrastructure.Persistance.MSSQL.Repositories
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        private readonly MSSQLDbContext _context;

        public UserRepository(MSSQLDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<User> ValidateUserByCredentialAsync(string email)
        {
            //return await _context.Users.SingleOrDefaultAsync(u => u.PrimaryEmail == email);
            
            return await _context.Users
                .Include(u => u.Profile) // Eagerly load the UserProfile
                .SingleOrDefaultAsync(u => u.PrimaryEmail == email);
        }
        
        public async Task<User> GetUserByEmailAsync(string email)
        {
            return await _context.Users.SingleOrDefaultAsync(u => u.PrimaryEmail == email);
        }
    }
}
