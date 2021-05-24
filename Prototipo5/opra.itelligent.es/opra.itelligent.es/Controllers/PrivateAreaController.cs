using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace opra.itelligent.es.Controllers
{
    [Authorize]
    public class PrivateAreaController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}