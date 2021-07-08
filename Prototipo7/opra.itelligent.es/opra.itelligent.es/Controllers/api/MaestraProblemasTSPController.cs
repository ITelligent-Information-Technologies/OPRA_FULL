using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.OData;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using opra.itelligent.es.Models;
using opra.itelligent.es.Services;
using opra.itelligent.es.ViewModels;

namespace opra.itelligent.es.Controllers.api
{
    [Authorize]
    public class MaestraProblemasTSPController : ODataController
    {
        private readonly BdOpraContext _context;
        private readonly VehicleRoutingService _vehicleRoutingService;

        public MaestraProblemasTSPController(BdOpraContext context, VehicleRoutingService vehicleRoutingService)
        {
            _context = context;
            _vehicleRoutingService = vehicleRoutingService;
        }

        // GET: api/MaestraProblemas
        [HttpGet]
        [EnableQuery]
        public IQueryable<TblMaestraProblemaTsp> Get()
        {
            return _context.TblMaestraProblemaTsp;
        }

        // GET: api/MaestraProblemas/5
        [EnableQuery]
        public async Task<ActionResult<TblMaestraProblemaTsp>> GetTblMaestraProblemaTsp(int key)
        {
            var TblMaestraProblemaTsp = await _context.TblMaestraProblemaTsp.FindAsync(key);

            if (TblMaestraProblemaTsp == null)
            {
                return NotFound();
            }

            return TblMaestraProblemaTsp;
        }

        // PUT: api/MaestraProblemas/5
        public async Task<IActionResult> Put([FromODataUri] int key, Delta<TblMaestraProblemaTsp> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            TblMaestraProblemaTsp TblMaestraProblemaTsp = _context.TblMaestraProblemaTsp.Find(key);
            if (TblMaestraProblemaTsp == null)
            {
                return NotFound();
            }

            patch.Put(TblMaestraProblemaTsp);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TblMaestraProblemaTspExists(key))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Updated(TblMaestraProblemaTsp);
        }
        

        // POST: api/MaestraProblemas
        [HttpPost]
        [EnableQuery]
        public async Task<IActionResult> PostTblMaestraProblemaTsp([FromBody]TblMaestraProblemaTsp TblMaestraProblemaTsp)
        {
            _context.TblMaestraProblemaTsp.Add(TblMaestraProblemaTsp);
            await _context.SaveChangesAsync();

            return Created(TblMaestraProblemaTsp);
        }

        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IActionResult> Patch([FromODataUri] int key, Delta<TblMaestraProblemaTsp> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            TblMaestraProblemaTsp TblMaestraProblemaTsp = await _context.TblMaestraProblemaTsp.FindAsync(key);
            if (TblMaestraProblemaTsp == null)
            {
                return NotFound();
            }

            patch.Patch(TblMaestraProblemaTsp);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TblMaestraProblemaTspExists(key))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Updated(TblMaestraProblemaTsp);
        }

        public async Task<IActionResult> Delete([FromODataUri] int key)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var TblMaestraProblemaTsp = await _context.TblMaestraProblemaTsp.FindAsync(key);
            if (TblMaestraProblemaTsp == null)
            {
                return NotFound();
            }

            _context.TblMaestraProblemaTsp.Remove(TblMaestraProblemaTsp);
            _context.SaveChanges();

            return NoContent();
        }

        private bool TblMaestraProblemaTspExists(int id)
        {
            return _context.TblMaestraProblemaTsp.Any(e => e.IntId == id);
        }

        public async Task<List<PointData>> CoordenadasOptimas([FromODataUri] int key)
        {
            List<PointData> coordOptimas = await _vehicleRoutingService.ObtenerCoordenadasOptimas(key);

            return coordOptimas;
        }
    }
}
