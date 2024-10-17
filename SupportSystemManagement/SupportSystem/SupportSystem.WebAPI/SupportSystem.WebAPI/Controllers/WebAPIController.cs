using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using SupportSystem.Backend.Interface.User;
using SupportSystem.Backend.SQLServer.Connection;
using SupportSystem.Backend.SQLServer.Factory;
using SupportSystem.User.Model;
using System.Data.SqlClient;
using SupportSystem.User.Factory;

namespace SupportSystem.WebAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class WebAPIController : ControllerBase
    {

        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WebAPIController> _logger;

        public WebAPIController(ILogger<WebAPIController> logger)
        {
            _logger = logger;
        }

        //    [HttpGet]
        //public IEnumerable<WeatherForecast> Get()
        //{
        //    var rng = new Random();
        //    return Enumerable.Range(1, 5).Select(index => new WeatherForecastg
        //    {
        //        Date = DateTime.Now.AddDays(index),
        //        TemperatureC = rng.Next(-20, 55),
        //        Summary = Summaries[rng.Next(Summaries.Length)]
        //    })
        //    .ToArray();
        //}


        [HttpGet]
        public string Get(string userid)
        {

             return "Web API Running";
        }

    }
}
