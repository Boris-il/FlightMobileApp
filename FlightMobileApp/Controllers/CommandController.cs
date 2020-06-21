using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FlightMobileApp.Model;
using FlightMobileApp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FlightMobileApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommandController : ControllerBase
    {
        FlightGearClient client;

        public CommandController()
        {
            this.client = new FlightGearClient();
        }

        /*// GET: api/Command
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }*/

        /*// GET: api/Command/5
        [HttpGet("{id}", Name = "Get")]
        public string Get(int id)
        {
            return "value";
        }*/

        [HttpPost]
        public async Task<ActionResult<Command>> PostCommand([FromBody] Command command)
        {
            // check query syntax
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid data.");
            }

            client.Start();
            await client.Execute(command);

            // returns success
            return StatusCode(200);
        }

        /*// PUT: api/Command/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }*/

        /*// DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }*/
    }
}
