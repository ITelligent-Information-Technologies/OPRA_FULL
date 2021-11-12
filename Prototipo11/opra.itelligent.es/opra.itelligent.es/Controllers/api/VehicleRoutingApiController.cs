using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using opra.itelligent.es.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace opra.itelligent.es.Controllers.api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class VehicleRoutingApiController : ControllerBase
    {
        [HttpPost("optimizar/{modelo}")]
        public async Task<IActionResult> Optimizar(int modelo, [FromBody] IEnumerable<PointData> datos)
        {
            //TODO
            await Task.CompletedTask;

            RespuestaTSP response = new RespuestaTSP
            {
                 Puntos = datos,
                 Coste = new Random().NextDouble() * 10,
                 Tiempo = new Random().NextDouble() * 2
            };

            return Ok(response);
        }
    }
}
