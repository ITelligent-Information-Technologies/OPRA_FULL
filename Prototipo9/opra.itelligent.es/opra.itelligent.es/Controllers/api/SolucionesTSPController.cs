using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.OData;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using opra.itelligent.es.Models;

namespace opra.itelligent.es.Controllers.api
{
    [Authorize]
    public class SolucionesTSPController : ODataController
    {
        private readonly BdOpraContext _context;

        public SolucionesTSPController(BdOpraContext context)
        {
            _context = context;
        }

        [HttpGet]
        [EnableQuery]
        public IQueryable<TblSolucionTsp> Get()
        {
            return _context.TblSolucionTsp;
        }

        [EnableQuery]
        public async Task<ActionResult<TblSolucionTsp>> GetTblSolucionTsp(int key)
        {
            var TblSolucionTsp = await _context.TblSolucionTsp.FindAsync(key);

            if (TblSolucionTsp == null)
            {
                return NotFound();
            }

            return TblSolucionTsp;
        }

        public async Task<IActionResult> Put([FromODataUri] int key, Delta<TblSolucionTsp> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            TblSolucionTsp TblSolucionTsp = _context.TblSolucionTsp.Find(key);
            if (TblSolucionTsp == null)
            {
                return NotFound();
            }

            patch.Put(TblSolucionTsp);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TblSolucionTspExists(key))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Updated(TblSolucionTsp);
        }
        

        [HttpPost]
        [EnableQuery]
        public async Task<IActionResult> PostTblSolucionTsp([FromBody]TblSolucionTsp TblSolucionTsp)
        {
            _context.TblSolucionTsp.Add(TblSolucionTsp);
            await _context.SaveChangesAsync();

            return Created(TblSolucionTsp);
        }

        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IActionResult> Patch([FromODataUri] int key, Delta<TblSolucionTsp> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            TblSolucionTsp TblSolucionTsp = await _context.TblSolucionTsp.FindAsync(key);
            if (TblSolucionTsp == null)
            {
                return NotFound();
            }

            patch.Patch(TblSolucionTsp);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TblSolucionTspExists(key))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Updated(TblSolucionTsp);
        }

        public async Task<IActionResult> Delete([FromODataUri] int key)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var TblSolucionTsp = await _context.TblSolucionTsp.FindAsync(key);
            if (TblSolucionTsp == null)
            {
                return NotFound();
            }

            _context.TblSolucionTsp.Remove(TblSolucionTsp);
            _context.SaveChanges();

            return NoContent();
        }

        private bool TblSolucionTspExists(int id)
        {
            return _context.TblSolucionTsp.Any(e => e.IntId == id);
        }
    }
}
