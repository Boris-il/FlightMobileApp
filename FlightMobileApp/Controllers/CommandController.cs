using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FlightMobileApp.Model;
using FlightMobileApp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace FlightMobileApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommandController : ControllerBase
    {
        FlightGearClient client;

        public CommandController(IConfiguration conf)
        {
            this.client = FlightGearClient.GetFlightGearClient(conf);

            //todo delete
            Console.WriteLine("inside controller");
        }

        /* // GET: api/Command
         [HttpGet]
         public IEnumerable<string> Get()
         {
             return new string[] { "value1", "value2" };
         }

         // GET: api/Command/5
         [HttpGet("{id}", Name = "Get")]
         public string Get(int id)
         {
             return "value";
         }*/

        [HttpPost]
        public async Task<ActionResult<Command>> PostCommand([FromBody] Command command)
        {
            try
            {
                // check query syntax
                if (!ModelState.IsValid)
                {
                    return BadRequest("Invalid data.");
                }

                //client.Start();
                var result = await client.Execute(command);
                return StatusCode((int)result);
            }
            catch (Exception e)
            {
                return NotFound(e.Message);
            }
        }

        // PUT: api/Command/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
