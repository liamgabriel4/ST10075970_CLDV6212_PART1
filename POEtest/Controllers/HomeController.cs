using Microsoft.AspNetCore.Mvc;
using POEtest.Models;
using System.Diagnostics;

namespace POEtest.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        // Check if the user is logged in before allowing access to the Index page
        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("Username") == null)
            {
                // If not logged in, redirect to the Login page
                return RedirectToAction("Login", "Account");
            }

            // If logged in, proceed to the Index view
            return View();
        }

        public IActionResult Privacy()
        {
            if (HttpContext.Session.GetString("Username") == null)
            {
                // Redirect to Login if user is not logged in
                return RedirectToAction("Login", "Account");
            }

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
    //Mrzyg?ód, K., 2022. Azure for Developers. 2nd ed. August: [Meeta Rajani]
}
