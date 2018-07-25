using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebAppPlugin1.Controllers
{
    public class Plugin1HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
