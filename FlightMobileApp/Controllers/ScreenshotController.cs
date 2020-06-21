using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace FlightMobileApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ScreenshotController : ControllerBase
    {
        string httpAddress, port;

        public ScreenshotController(IConfiguration conf)
        {
            this.httpAddress = conf.GetValue<string>("Logging:ScreensConnectionData:Host");
            this.port = conf.GetValue<string>("Logging:ScreensConnectionData:Port");
        }


        // GET: api/Screenshot
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var client = new HttpClient
            {
                // Set timeout.
                Timeout = TimeSpan.FromSeconds(8000)
            };

            try
            {
                // Build request string.
                string request = "http://" + this.httpAddress + ":" + this.port + "/screenshot";
                // Send http GET request.
                var response = await client.GetAsync(request);
                // Await response and convert to image.
                var img = await response.Content.ReadAsStreamAsync();
                var screenshot = File(img, "img/jpg");
                return screenshot;

            }
            catch (Exception)
            {
                // Else return error code.
                return NotFound("Error in http request to FlightGear\n");

            }
        }

    }
}
