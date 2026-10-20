namespace SS.Base.Application.Queries;
using MediatR;
using SS.Base.Domain.Dto;

// Every Support System account, as the admin User List shows it (never the profile or credentials).
public class GetAllUsersQuery : IRequest<IEnumerable<UserDto>>
{
}
