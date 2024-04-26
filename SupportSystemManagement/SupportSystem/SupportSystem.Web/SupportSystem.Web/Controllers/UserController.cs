using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SupportSystem.Web.Controllers
{
    public class UserController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GetDetails(UserInputModel userInput)
        {

            HttpClient httpClient = new HttpClient();
            string webAPIURL = (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("webapiurl")))? "http://" + Environment.GetEnvironmentVariable("webapiurl")+ "/user/createagentuser" : "https://localhost:44384/user/createagentuser";
            // Serialize the data to JSON format
            var json = JsonConvert.SerializeObject(userInput);
            await PostDataAsync(webAPIURL, json);
            // Create the request body from the serialized JSON data
            var requestBody = new StringContent(json, Encoding.UTF8, "application/json");

            return View();
        }


        public static async Task PostDataAsync(string url, string jsonData)
        {
            try
            {
                // Prepare the HTTP request content
                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                // Create a new HttpClient instance
                using (var httpClient = new HttpClient())
                {
                    // Make the POST request and get the response
                    var response = await httpClient.PostAsync(url, content);

                    // Check if the request was successful
                    if (response.IsSuccessStatusCode)
                    {
                        // Read the response content as string
                        string responseContent = await response.Content.ReadAsStringAsync();
                        Console.WriteLine("Response: " + responseContent);
                    }
                    else
                    {
                        Console.WriteLine($"Failed to call the Web API. Status code: {response.StatusCode}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }
        }




        public async Task<IActionResult> GetUsers()
        {
              HttpClient _httpClient;
            _httpClient = new HttpClient();
            string webAPIURL = (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("webapiurl"))) ? "http://" + Environment.GetEnvironmentVariable("webapiurl") + "/" : "https://localhost:44384/";
            _httpClient.BaseAddress = new Uri(webAPIURL); // Replace with your Web API base URL
            try
            {
                // Make GET request to your Web API endpoint
                HttpResponseMessage response = await _httpClient.GetAsync("weatherforecast");

                // Check if the request was successful
                if (response.IsSuccessStatusCode)
                {
                    // Read and parse the response content
                    string content = await response.Content.ReadAsStringAsync();
                    // Process the response content as needed
                    return Ok(content);
                }
                else
                {
                    // Handle the error response
                    return StatusCode((int)response.StatusCode, response.ReasonPhrase);
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occur during the request
                return StatusCode(500, ex.Message);
            }
        }

    }
    public class UserInputModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string Country { get; set; }
        public string Gender { get; set; }
        public string PrimaryNumber { get; set; }
        public string PrimaryEmail { get; set; }
    }
}
