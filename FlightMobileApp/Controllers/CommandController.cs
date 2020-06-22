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
        readonly FlightGearClient  client;

        public CommandController()
        {
            this.client = FlightGearClient.GetFlightGearClient();
            
        }


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

    
    }
}
