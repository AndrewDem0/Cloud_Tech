using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;
using WebApplication_lab.Models;

namespace WebApplication_lab.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public static UserEntra GetUserOnEntraID(ClaimsPrincipal user)
        {
            // Retrieve the preferred_username claim from the user's claims
            var preferredUsernameClaim = user.Claims.FirstOrDefault(c => c.Type.Equals("preferred_username"));
            if (preferredUsernameClaim != null)
            {
                return new UserEntra
                {
                    user_name = user.Claims.FirstOrDefault(p => p.Type.Equals("name")).Value,
                    user_email = preferredUsernameClaim.Value,
                };
            }
            return null; // throwing an exception if preferred_username claim is required
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
