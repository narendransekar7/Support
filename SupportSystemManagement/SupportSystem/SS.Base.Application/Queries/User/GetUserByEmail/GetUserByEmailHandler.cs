namespace SS.Base.Application.Queries;

using MediatR;
using SS.Base.Application.Commands;
using SS.Base.Domain.Entities;
using SS.Base.Domain.Interfaces.Repository;

public class GetUserByEmailHandler : IRequestHandler<GetUserByEmailQuery, User>
{
    private readonly IUserRepository _userRepository;

    public GetUserByEmailHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<User> Handle(GetUserByEmailQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetUserByEmailAsync(request.Email);

        if (user == null)
        {
            throw new KeyNotFoundException("User not found.");
        }
        return user;
    }
    
}