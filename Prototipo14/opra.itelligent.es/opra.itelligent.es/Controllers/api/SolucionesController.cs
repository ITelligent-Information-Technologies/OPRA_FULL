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
    public class SolucionesController : ODataController
    {
        private readonly BdOpraContext _context;

        public SolucionesController(BdOpraContext context)
        {
            _context = context;
        }

        [HttpGet]
        [EnableQuery]
        public IQueryable<TblSolucion> Get()
        {
            return _context.TblSolucion;
        }

        [EnableQuery]
        public async Task<ActionResult<TblSolucion>> GetTblSolucion(int key)
        {
            var TblSolucion = await _context.TblSolucion.FindAsync(key);

            if (TblSolucion == null)
            {
                return NotFound();
            }

            return TblSolucion;
        }

        public async Task<IActionResult> Put([FromODataUri] int key, Delta<TblSolucion> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            TblSolucion TblSolucion = _context.TblSolucion.Find(key);
            if (TblSolucion == null)
            {
                return NotFound();
            }

            patch.Put(TblSolucion);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TblSolucionExists(key))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Updated(TblSolucion);
        }
        

        [HttpPost]
        [EnableQuery]
        public async Task<IActionResult> PostTblSolucion([FromBody]TblSolucion TblSolucion)
        {
            _context.TblSolucion.Add(TblSolucion);
            await _context.SaveChangesAsync();

            return Created(TblSolucion);
        }

        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IActionResult> Patch([FromODataUri] int key, Delta<TblSolucion> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            TblSolucion TblSolucion = await _context.TblSolucion.FindAsync(key);
            if (TblSolucion == null)
            {
                return NotFound();
            }

            patch.Patch(TblSolucion);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TblSolucionExists(key))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Updated(TblSolucion);
        }

        public async Task<IActionResult> Delete([FromODataUri] int key)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var TblSolucion = await _context.TblSolucion.FindAsync(key);
            if (TblSolucion == null)
            {
                return NotFound();
            }

            _context.TblSolucion.Remove(TblSolucion);
            _context.SaveChanges();

            return NoContent();
        }

        private bool TblSolucionExists(int id)
        {
            return _context.TblSolucion.Any(e => e.IntId == id);
        }
    }
}
