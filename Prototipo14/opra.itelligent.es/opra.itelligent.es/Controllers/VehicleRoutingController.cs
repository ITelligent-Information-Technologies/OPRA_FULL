using Amazon.S3;
using Amazon.S3.Model;
using Geolocation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using opra.itelligent.es.Models;
using opra.itelligent.es.Services;
using opra.itelligent.es.ViewModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace opra.itelligent.es.Controllers
{
    public class VehicleRoutingController : Controller
    {
        private readonly BdOpraContext _context;
        private readonly VehicleRoutingService _vehicleRoutingService;

        public VehicleRoutingController(BdOpraContext context, VehicleRoutingService vehicleRoutingService)
        {
            _context = context;
            _vehicleRoutingService = vehicleRoutingService;
        }

        public async Task<IActionResult> Index(int problema)
        {
            TblMaestraProblemaTsp datosProblema = _context.TblMaestraProblemaTsp.FirstOrDefault(x => x.IntId == problema);

            List<int> ordenOptimo = JsonConvert.DeserializeObject<List<int>>(datosProblema.StrSolucionOptima);
            ordenOptimo.Add(ordenOptimo[0]);

            VehicleRoutingModel model = new VehicleRoutingModel
            {
                Problema = problema,
                Puntos = new List<PointData>(),
                SolucionOptima = ordenOptimo
            };

            model.Puntos = await _vehicleRoutingService.ObtenerCoordenadas(datosProblema);

            return View(model);
        }
    }
}
