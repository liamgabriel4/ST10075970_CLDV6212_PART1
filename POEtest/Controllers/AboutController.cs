using Microsoft.AspNetCore.Mvc;

namespace POEtest.Controllers
{
    public class AboutController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
