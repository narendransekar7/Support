namespace SS.Base.Application.Queries;

using MediatR;
using SS.Base.Domain.Dto;
using SS.Base.Domain.Interfaces.Repository;

public class GetAllUsersHandler : IRequestHandler<GetAllUsersQuery, IEnumerable<UserDto>>
{
    private readonly IUserRepository _userRepository;

    public GetAllUsersHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<IEnumerable<UserDto>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await _userRepository.GetAllAsync();

        // Project onto the DTO so the profile (password hash) never leaves the service.
        return users
            .OrderBy(u => u.DisplayName)
            .Select(u => new UserDto
            {
                UserId = u.UserId,
                Name = u.DisplayName,
                Email = u.PrimaryEmail,
                Role = u.Role
            })
            .ToList();
    }
}
