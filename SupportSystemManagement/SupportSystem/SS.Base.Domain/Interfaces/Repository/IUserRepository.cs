using SS.Base.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SS.Base.Domain.Interfaces.Repository
{
    public interface IUserRepository: IGenericRepository<User>
    {
        // Add methods specific to Users like UpdatePassowrd,ValidateUser. since other methods generic method are available in IGenericRepository and (GenericRepositoryAddAsync,GetByIdAsync,GetAllAsync)  
        Task<User> ValidateUserByCredentialAsync(string Email);
        Task<User?> GetUserByEmailAsync(string email);
    }
}
