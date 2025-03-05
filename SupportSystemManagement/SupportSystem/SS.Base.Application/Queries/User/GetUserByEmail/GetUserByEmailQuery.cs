namespace SS.Base.Application.Queries;
using MediatR;
using SS.Base.Domain.Entities;
public class GetUserByEmailQuery:IRequest<User>
{
    public string Email { get; set; }
    
    public GetUserByEmailQuery(string userEmail)
    {
        Email = userEmail;
    }
}