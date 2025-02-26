namespace SS.Base.Application.Commands;

using MediatR;
using SS.Base.Domain.Interfaces.Repository;
using SS.Base.Domain.Entities;

public class LoginSuccessHandler: IRequestHandler<LoginSuccessCommand>
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUnitOfWork _unitOfWork;

    public LoginSuccessHandler(IRefreshTokenRepository refreshTokenRepository, IUnitOfWork unitOfWork)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(LoginSuccessCommand request, CancellationToken cancellationToken)
    {
        var refreshToken = new RefreshToken
        {
            Token =  Guid.NewGuid(),
            UserId = request.UserId,
            ExpiryDate = DateTime.Now.AddDays(2),
            IsRevoked = false
        };
        // Use the repository to add the user
        await _refreshTokenRepository.AddAsync(refreshToken);

        // Commit changes using Unit of Work
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}