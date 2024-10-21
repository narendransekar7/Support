using Microsoft.AspNetCore.Mvc;

namespace SupportSystem.User.API.Controllers;

public class UserAPIController : ControllerBase
{

    [HttpGet]
    public string Get(string userid)
    {

        return "User API Running";
    }
}