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

        public async Task<User?> GetNextAgentForAssignmentAsync()
        {
            var agents = await _context.Users
                .Where(u => u.Role == Role.Agent)
                .OrderBy(u => u.UserId)
                .ToListAsync();

            if (agents.Count == 0)
            {
                return null;
            }

            var cursor = await _context.RoundRobinCursors.SingleOrDefaultAsync(c => c.Id == 1);
            if (cursor == null)
            {
                cursor = new RoundRobinCursor { Id = 1, LastAssignedUserId = null };
                await _context.RoundRobinCursors.AddAsync(cursor);
            }

            var lastIndex = cursor.LastAssignedUserId.HasValue
                ? agents.FindIndex(a => a.UserId == cursor.LastAssignedUserId.Value)
                : -1;

            var nextAgent = agents[(lastIndex + 1) % agents.Count];
            cursor.LastAssignedUserId = nextAgent.UserId;

            return nextAgent;
        }
    }
}
