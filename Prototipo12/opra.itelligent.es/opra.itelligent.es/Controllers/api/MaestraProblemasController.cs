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
    public class MaestraProblemasController : ODataController
    {
        private readonly BdOpraContext _context;

        public MaestraProblemasController(BdOpraContext context)
        {
            _context = context;
        }

        // GET: api/MaestraProblemas
        [HttpGet]
        [EnableQuery]
        public IQueryable<TblMaestraProblema> Get()
        {
            return _context.TblMaestraProblema;
        }

        // GET: api/MaestraProblemas/5
        [EnableQuery]
        public async Task<ActionResult<TblMaestraProblema>> GetTblMaestraProblema(int key)
        {
            var tblMaestraProblema = await _context.TblMaestraProblema.FindAsync(key);

            if (tblMaestraProblema == null)
            {
                return NotFound();
            }

            return tblMaestraProblema;
        }

        // PUT: api/MaestraProblemas/5
        public async Task<IActionResult> Put([FromODataUri] int key, Delta<TblMaestraProblema> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            TblMaestraProblema tblMaestraProblema = _context.TblMaestraProblema.Find(key);
            if (tblMaestraProblema == null)
            {
                return NotFound();
            }

            patch.Put(tblMaestraProblema);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TblMaestraProblemaExists(key))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Updated(tblMaestraProblema);
        }
        

        // POST: api/MaestraProblemas
        [HttpPost]
        [EnableQuery]
        public async Task<IActionResult> PostTblMaestraProblema([FromBody]TblMaestraProblema tblMaestraProblema)
        {
            _context.TblMaestraProblema.Add(tblMaestraProblema);
            await _context.SaveChangesAsync();

            return Created(tblMaestraProblema);
        }

        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IActionResult> Patch([FromODataUri] int key, Delta<TblMaestraProblema> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            TblMaestraProblema tblMaestraProblema = await _context.TblMaestraProblema.FindAsync(key);
            if (tblMaestraProblema == null)
            {
                return NotFound();
            }

            patch.Patch(tblMaestraProblema);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TblMaestraProblemaExists(key))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Updated(tblMaestraProblema);
        }

        public async Task<IActionResult> Delete([FromODataUri] int key)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var tblMaestraProblema = await _context.TblMaestraProblema.FindAsync(key);
            if (tblMaestraProblema == null)
            {
                return NotFound();
            }

            _context.TblMaestraProblema.Remove(tblMaestraProblema);
            _context.SaveChanges();

            return NoContent();
        }

        private bool TblMaestraProblemaExists(int id)
        {
            return _context.TblMaestraProblema.Any(e => e.IntId == id);
        }
    }
}
