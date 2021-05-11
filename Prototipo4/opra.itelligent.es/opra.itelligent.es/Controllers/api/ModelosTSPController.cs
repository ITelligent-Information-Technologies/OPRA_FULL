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
    public class ModelosTSPController : ODataController
    {
        private readonly BdOpraContext _context;

        public ModelosTSPController(BdOpraContext context)
        {
            _context = context;
        }

        // GET: api/MaestraProblemas
        [HttpGet]
        [EnableQuery]
        public IQueryable<TblModeloTsp> Get()
        {
            return _context.TblModeloTsp;
        }

        // GET: api/MaestraProblemas/5
        [EnableQuery]
        public async Task<ActionResult<TblModeloTsp>> GetTblModeloTsp(int key)
        {
            var TblModeloTsp = await _context.TblModeloTsp.FindAsync(key);

            if (TblModeloTsp == null)
            {
                return NotFound();
            }

            return TblModeloTsp;
        }

        // PUT: api/MaestraProblemas/5
        public async Task<IActionResult> Put([FromODataUri] int key, Delta<TblModeloTsp> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            TblModeloTsp TblModeloTsp = _context.TblModeloTsp.Find(key);
            if (TblModeloTsp == null)
            {
                return NotFound();
            }

            patch.Put(TblModeloTsp);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TblModeloTspExists(key))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Updated(TblModeloTsp);
        }

        private bool TblModeloTspExists(int id)
        {
            return _context.TblModeloTsp.Any(e => e.IntId == id);
        }
    }
}
