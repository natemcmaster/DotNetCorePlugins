using Microsoft.AspNetCore.Mvc;

namespace MvcWebApp.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
