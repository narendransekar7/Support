using SS.Base.Domain.Entities;

namespace SS.Base.Domain.Interfaces.Repository;

public interface IRefreshTokenRepository: IGenericRepository<RefreshToken>
{
    Task<RefreshToken?> GetByIdAsync(Guid id);
}