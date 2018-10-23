using Microsoft.AspNetCore.Mvc;

namespace MvcAppPlugin1
{
    public class MyPluginController : Controller
    {
        public IActionResult Index() => View();
    }
}
