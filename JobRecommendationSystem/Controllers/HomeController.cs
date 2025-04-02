using Microsoft.AspNetCore.Mvc;

namespace JobRecommendationSystem.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
